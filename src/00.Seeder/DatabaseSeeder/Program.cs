using DatabaseSeeder;
using DatabaseSeeder.Interfaces;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using IAM.Infrastructure;
using IAM.Infrastructure.Repositories;
using IAM.Infrastructure.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Contracts;
using Shared.Infrastructure;

var connectionString =
   Environment.GetEnvironmentVariable("ConnectionStrings__IamDb")
   ?? Environment.GetEnvironmentVariable("IAM_DB_CONNECTION")
   ?? "Host=127.0.0.1;Port=5432;Database=iam;Username=admin;Password=cmsadmin123";

var services = new ServiceCollection();

services.AddDbContext<IamDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
services.AddDbContext<SharedDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

services.AddScoped<IUserContext, SeederUserContext>();
services.AddScoped<IIamUnitOfWork, IamUnitOfWork>();
services.AddScoped<IOrganizationRepository, OrganizationRepository>();
services.AddScoped<IRoleRepository, RoleRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IPermissionRepository, PermissionRepository>();

services.AddScoped<IDatabaseSeeder, IamDatabaseSeeder>();

await using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();

var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
await seeder.SeedAsync();

Console.WriteLine("Database seed completed.");
