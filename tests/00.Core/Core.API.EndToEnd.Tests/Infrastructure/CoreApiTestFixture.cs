extern alias CoreApi;

using Courier.Infrastructure.BackgroundServices;
using IAM.Domain;
using IAM.Domain.Entities;
using IAM.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Infrastructure.BackgroundServices;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Infrastructure;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Core.API.EndToEnd.Tests.Infrastructure;

public sealed class CoreApiTestFixture : WebApplicationFactory<CoreApi::Program>, IAsyncLifetime
{
   private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
      .WithImage("postgres:16-alpine")
      .WithDatabase("iam")
      .WithUsername("postgres")
      .WithPassword("postgres")
      .Build();

   private readonly RedisContainer _redis = new RedisBuilder()
      .WithImage("redis:7-alpine")
      .Build();

   public CoreApiClient Api { get; private set; } = null!;

   public async ValueTask InitializeAsync()
   {
      await _postgres.StartAsync();
      await _redis.StartAsync();

      var client = CreateClient();
      Api = new CoreApiClient(client);

      await MigrateAsync();
      await SeedAsync();
   }

   public override async ValueTask DisposeAsync()
   {
      await _redis.DisposeAsync();
      await _postgres.DisposeAsync();
      await base.DisposeAsync();
   }

   protected override void ConfigureWebHost(IWebHostBuilder builder)
   {
      builder.UseEnvironment("EndToEnd");
      builder.ConfigureAppConfiguration((_, configuration) =>
      {
         configuration.AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["ConnectionStrings:IamDb"] = _postgres.GetConnectionString(),
            ["ConnectionStrings:SharedDb"] = _postgres.GetConnectionString(),
            ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
            ["ConnectionStrings:SentinelDb"] = "mongodb://localhost:27017",
            ["ConnectionStrings:CourierDb"] = "mongodb://localhost:27017",
            ["Jwt:Secret"] = "your-super-secret-jwt-key-here-make-it-long-and-secure-at-least-32-characters",
            ["Jwt:ExpirationHours"] = "24",
            ["InternalApi:Key"] = "test-key",
            ["Sentinel:HostedServicesEnabled"] = "false",
            ["Courier:HostedServicesEnabled"] = "false"
         });
      });
      builder.ConfigureServices(services =>
      {
         services.RemoveHostedService<AuditLogConsumer>();
         services.RemoveHostedService<SystemLogConsumer>();
         services.RemoveHostedService<CourierIndexInitializer>();
         services.RemoveHostedService<EmailRequestConsumer>();
         services.RemoveHostedService<EmailDeliveryWorker>();
      });
   }

   private async Task MigrateAsync()
   {
      using var scope = Services.CreateScope();
      var iamDbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();
      var sharedDbContext = scope.ServiceProvider.GetRequiredService<SharedDbContext>();

      await sharedDbContext.Database.MigrateAsync();
      await iamDbContext.Database.MigrateAsync();
   }

   private async Task SeedAsync()
   {
      using var scope = Services.CreateScope();
      var iamDbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();
      var sharedDbContext = scope.ServiceProvider.GetRequiredService<SharedDbContext>();

      if (await iamDbContext.Permissions.AnyAsync())
      {
         return;
      }

      var permissions = CreateIamPermissions();
      iamDbContext.Permissions.AddRange(permissions);

      var organizationAdminRole = Role.Create("Organization Admin", "Access to organization resources.", false, true, null);
      var userRole = Role.Create("User", "Access to own resources.", true, true, null);

      foreach (var permission in permissions)
      {
         organizationAdminRole.AddPermission(permission.Id);
      }

      userRole.AddPermission(permissions.Single(permission => permission.Code == IamPermission.Users.UpdateMe).Id);
      userRole.AddPermission(permissions.Single(permission => permission.Code == IamPermission.Users.DeleteMe).Id);
      userRole.AddPermission(permissions.Single(permission => permission.Code == IamPermission.Users.UpdatePassword).Id);

      iamDbContext.Roles.AddRange(organizationAdminRole, userRole);
      await iamDbContext.SaveChangesAsync();

      sharedDbContext.Parameters.AddRange(
         CreateParameter(IamParam.Security.MaxPasswordAgeInDays, ParameterType.Integer, "90", ParameterOverrideType.UserOwnerId),
         CreateParameter(IamParam.Security.LockoutDurationInMins, ParameterType.Integer, "60", ParameterOverrideType.None),
         CreateParameter(IamParam.Security.MaxFailedLoginAttempts, ParameterType.Integer, "3", ParameterOverrideType.None),
         CreateParameter(IamParam.Security.JwtExpirationInHours, ParameterType.Integer, "24", ParameterOverrideType.None),
         CreateParameter(IamParam.Role.DefaultRoleIdForNewOrganization, ParameterType.UUID, organizationAdminRole.Id.ToString(), ParameterOverrideType.None),
         CreateParameter(IamParam.Role.DefaultRoleIdForNewUser, ParameterType.UUID, userRole.Id.ToString(), ParameterOverrideType.None));

      await sharedDbContext.SaveChangesAsync();
   }

   private static List<Permission> CreateIamPermissions()
   {
      return typeof(IamPermission)
         .GetNestedTypes()
         .SelectMany(type => type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
         .Where(field => field.IsLiteral && field.FieldType == typeof(string))
         .Select(field => (string)field.GetRawConstantValue()!)
         .Select(CreatePermission)
         .ToList();
   }

   private static Permission CreatePermission(string code)
   {
      var parts = code.Split('.');

      return Permission.Create(
         parts[0],
         parts[1],
         parts[2],
         code,
         code,
         isActive: true);
   }

   private static Parameter CreateParameter(
      string key,
      ParameterType type,
      string value,
      ParameterOverrideType overrideType)
   {
      var parameterKey = new ParameterKey(key);

      return Parameter.Create(
         parameterKey.Module,
         parameterKey.Group,
         parameterKey.Name,
         parameterKey.Name,
         parameterKey.Key,
         type,
         value,
         validationRegex: null,
         validationErrorCustomMessage: null,
         listItems: null,
         externalListEndpoint: null,
         overrideType,
         isVisible: true);
   }
}

internal static class ServiceCollectionExtensions
{
   public static void RemoveHostedService<TImplementation>(this IServiceCollection services)
   {
      var descriptors = services
         .Where(descriptor => descriptor.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService)
            && descriptor.ImplementationType == typeof(TImplementation))
         .ToArray();

      foreach (var descriptor in descriptors)
      {
         services.Remove(descriptor);
      }
   }
}
