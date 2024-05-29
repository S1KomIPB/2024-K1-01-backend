## Packages

1. dotnet add package Microsoft.EntityFrameworkCore.SqlServer -v 7.0.0
2. dotnet add package DotNetEnv

## Database

1. Setting database authentication -> **Windows Authentication**
2. In Databases folder, add new database "capstone"

## Initialisation
```bash
cp secrets.json.example secrets.json
type ./secrets.json | dotnet user-secrets set --project WebApi
dotnet ef database update --project WebApi
```

## How to run server

```bash
dotnet run --project WebApi
```