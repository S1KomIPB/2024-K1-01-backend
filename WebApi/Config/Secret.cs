using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Config
{
    public static  class Secret
    {
        public static string JWTIssuer { get; private set; } = string.Empty;
        public static string JWTAudience { get; private set; } = string.Empty;
        public static SymmetricSecurityKey JWTSecretKey { get; private set; } = new SymmetricSecurityKey(new byte[1]);
        public static int JWTExpirationInMinutes { get; private set; }
        public static string AdminName { get; private set; } = string.Empty;
        public static string AdminInitials { get; private set; } = string.Empty;
        public static string AdminPassword { get; private set; } = string.Empty;
        public static string ConnectionString { get; private set; } = string.Empty;
        
        private static string GetRequiredSetting(IConfiguration configuration, string key)
        {
            var setting = configuration[key];
            if (string.IsNullOrEmpty(setting))
            {
                throw new Exception($"{key} is missing in appsettings.json");
            }
            return setting;
        }

        public static void Initialize(IConfiguration configuration)
        {
            JWTIssuer = GetRequiredSetting(configuration, "Secret:JWTIssuer");
            JWTAudience = GetRequiredSetting(configuration, "Secret:JWTAudience");
            JWTSecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetRequiredSetting(configuration, "Secret:JWTSecretKey")));
            JWTExpirationInMinutes = int.Parse(GetRequiredSetting(configuration, "Secret:JWTExpirationInMinutes"));
            AdminName = GetRequiredSetting(configuration, "Secret:AdminName");
            AdminInitials = GetRequiredSetting(configuration, "Secret:AdminInitials");
            AdminPassword = GetRequiredSetting(configuration, "Secret:AdminPassword");
            ConnectionString = GetRequiredSetting(configuration, "Secret:ConnectionString");
        }
    }
}