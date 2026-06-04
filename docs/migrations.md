# Migration Guide

This guide outlines the steps to manage database migrations for the IAM.API project using Entity Framework Core.

## 1. Verify connection string
Ensure `src/03.IAM/IAM.API/appsettings.json` or `appsettings.Development.json` contains the connection under `ConnectionStrings:IamDb`.
`{
  "ConnectionStrings": {
    "IAM": "Host=localhost;Database=iam;Username=postgres;Password=yourpassword"
  }
}`

## 2. Ensure EF tooling & design package are available
Install CLI tooling if you don't have it:
`dotnet tool install --global dotnet-ef`

## 3. Create Database from scratch
If you want to create a database from scratch, delete migration files from the target infrastructure project and run:
Shared: `dotnet ef migrations add InitialCreate --project src/01.Shared/Shared.Infrastructure --startup-project src/01.Shared/Shared.Infrastructure -o Migrations`
IAM: `dotnet ef migrations add InitialCreate --project src/03.IAM/IAM.Infrastructure --startup-project src/03.IAM/IAM.API -o Migrations --context IamDbContext`

## 4. Inspect migrations (optional)
To list existing migrations, run the following command from the root solution directory:
Shared: `dotnet ef migrations list --project src/01.Shared/Shared.Infrastructure --startup-project src/01.Shared/Shared.Infrastructure`
IAM: `dotnet ef migrations list --project src/03.IAM/IAM.Infrastructure --startup-project src/03.IAM/IAM.API --context IamDbContext`

## 5. Add new migrations after model changes
After making changes to your entity models, create a new migration with the following command from the root solution directory:
Shared: `dotnet ef migrations add [MigrationName] --project src/01.Shared/Shared.Infrastructure --startup-project src/01.Shared/Shared.Infrastructure -o Migrations`
IAM: `dotnet ef migrations add [MigrationName] --project src/03.IAM/IAM.Infrastructure --startup-project src/03.IAM/IAM.API -o Migrations --context IamDbContext`

## 6. Apply migrations to update the database (after steps #3 or #5)
Run the following command from the root solution directory to apply migrations:
Shared: `dotnet ef database update --project src/01.Shared/Shared.Infrastructure --startup-project src/01.Shared/Shared.Infrastructure`
IAM: `dotnet ef database update --project src/03.IAM/IAM.Infrastructure --startup-project src/03.IAM/IAM.API --context IamDbContext`

## 7. Rollback to a previous migration (if needed)
To rollback to a previous migration, run the following command from the root solution directory, replacing YourMigrationName with the target migration:
Shared: `dotnet ef database update [MigrationName] --project src/01.Shared/Shared.Infrastructure --startup-project src/01.Shared/Shared.Infrastructure`
IAM: `dotnet ef database update [MigrationName] --project src/03.IAM/IAM.Infrastructure --startup-project src/03.IAM/IAM.API --context IamDbContext`

## 8. Drop the database (if needed)
To drop the database, run the following command from the root solution directory:
Shared: `dotnet ef database drop --project src/01.Shared/Shared.Infrastructure --startup-project src/01.Shared/Shared.Infrastructure`
IAM: `dotnet ef database drop --project src/03.IAM/IAM.Infrastructure --startup-project src/03.IAM/IAM.API --context IamDbContext`

## Troubleshooting
If the CLI picks the wrong environment (so appsettings.Development.json isn't used), set it inline:
Shared: `ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/01.Shared/Shared.Infrastructure --startup-project src/01.Shared/Shared.Infrastructure`
IAM: `ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/03.IAM/IAM.Infrastructure --startup-project src/03.IAM/IAM.API --context IamDbContext`

- Make sure the DB user has rights to create a database/schema.
- If you see no pending migrations, confirm the migration files exist (e.g. Shared.Infrastructure/Migrations) and that DbContext (e.g. SharedDbContext) in infrastructure project (e.g. Shared.Infrastructure) is the one referenced by your migrations (snapshot shows it is).
- After migrations succeed, your development Program.cs seeder will run (you already call seeder.SeedAsync() when in Development).
