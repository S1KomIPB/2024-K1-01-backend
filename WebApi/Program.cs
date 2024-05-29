using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Config;
using WebApi.Data;
using WebApi.Models;

var builder = WebApplication.CreateBuilder(args);
Secret.Initialize(builder.Configuration);

static void CheckDatabaseConnection(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    try
    {
        context.Database.OpenConnection();
        context.Database.CloseConnection();
        Console.WriteLine("Database connection is successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to connect to the database: {ex.Message}");
        Environment.Exit(-1);
    }
}

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Secret.JWTIssuer,
        ValidAudience = Secret.JWTAudience,
        IssuerSigningKey = Secret.JWTSecretKey
    };
});

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(Secret.ConnectionString));

var app = builder.Build();

//check connection to the db
CheckDatabaseConnection(app.Services);

async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    var adminExists = await context.Users.AnyAsync(u => u.IsAdmin && u.IsActive);
    var usersCount = await context.Users.CountAsync();
    if (!adminExists)
    {

        var user = new User
        {
            Name = Secret.AdminName,
            InitialChar = Secret.AdminInitials,
            IsAdmin = true,
            Password = Secret.AdminPassword,
            Email = "",
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}

await InitializeDatabaseAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();