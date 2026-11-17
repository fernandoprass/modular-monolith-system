using DatabaseSeeder;
using DatabaseSeeder.Interfaces;
using Courier.Domain.Interfaces.Repositories;
using Courier.Infrastructure;
using Courier.Infrastructure.Repositories;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using IAM.Infrastructure;
using IAM.Infrastructure.Repositories;
using IAM.Infrastructure.UoW;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Contracts;
using Shared.Infrastructure;

var configuration = new ConfigurationBuilder()
   .SetBasePath(AppContext.BaseDirectory)
   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
   .Build();

var connectionString =
   Environment.GetEnvironmentVariable("ConnectionStrings__IamDb")
   ?? Environment.GetEnvironmentVariable("IAM_DB_CONNECTION")
   ?? configuration.GetConnectionString("IamDb")
   ?? throw new InvalidOperationException("IAM database connection string is required.");

var courierConnectionString =
   Environment.GetEnvironmentVariable("ConnectionStrings__CourierDb")
   ?? Environment.GetEnvironmentVariable("COURIER_DB_CONNECTION")
   ?? configuration.GetConnectionString("CourierDb")
   ?? throw new InvalidOperationException("Courier database connection string is required.");

var courierDatabaseName =
   Environment.GetEnvironmentVariable("Courier__DatabaseName")
   ?? Environment.GetEnvironmentVariable("COURIER_DATABASE_NAME")
   ?? configuration["Courier:DatabaseName"]
   ?? "courier";

configuration["ConnectionStrings:IamDb"] = connectionString;
configuration["ConnectionStrings:CourierDb"] = courierConnectionString;
configuration["Courier:DatabaseName"] = courierDatabaseName;

var services = new ServiceCollection();

services.AddDbContext<IamDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
services.AddDbContext<SharedDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<ISeederData,SeederData>();
services.AddScoped<IUserContext, SeederUserContext>();
services.AddScoped<IIamUnitOfWork, IamUnitOfWork>();
services.AddScoped<IOrganizationRepository, OrganizationRepository>();
services.AddScoped<IRoleRepository, RoleRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IPermissionRepository, PermissionRepository>();
services.AddSingleton<CourierDbContext>();
services.AddScoped<ITemplateRepository, TemplateRepository>();
services.AddScoped<ITemplateWriteRepository, TemplateRepository>();

services.AddScoped<IDatabaseSeeder, DatabaseSeeder.DatabaseSeeder>();

await using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();

var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
await seeder.SeedAsync();

Console.WriteLine("Database seed completed.");
