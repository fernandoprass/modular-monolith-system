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
         await EnsureRoleDeletePermissionAsync(iamDbContext, sharedDbContext);
         await EnsureDefaultUserPermissionsAsync(iamDbContext, sharedDbContext);
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
      await EnsureRoleDeletePermissionAsync(iamDbContext, sharedDbContext);
      await EnsureDefaultUserPermissionsAsync(iamDbContext, sharedDbContext);
   }

   private static async Task EnsureDefaultUserPermissionsAsync(IamDbContext iamDbContext, SharedDbContext sharedDbContext)
   {
      var requiredPermissionCodes = new[]
      {
         IamPermission.Users.UpdateMe,
         IamPermission.Users.DeleteMe,
         IamPermission.Users.UpdatePassword
      };

      foreach (var permissionCode in requiredPermissionCodes)
      {
         var permissionExists = await iamDbContext.Permissions.AnyAsync(permission => permission.Code == permissionCode);
         if (!permissionExists)
         {
            iamDbContext.Permissions.Add(CreatePermission(permissionCode));
         }
      }

      await iamDbContext.SaveChangesAsync();

      var userRoleIdValue = await sharedDbContext.Parameters
         .Where(parameter => parameter.Key == IamParam.Role.DefaultRoleIdForNewUser)
         .Select(parameter => parameter.Value)
         .FirstOrDefaultAsync();

      if (!Guid.TryParse(userRoleIdValue, out var userRoleId))
      {
         var userRole = await iamDbContext.Roles.FirstOrDefaultAsync(role => role.Name == "User" && role.OrganizationId == null);
         if (userRole == null)
         {
            return;
         }

         userRoleId = userRole.Id;
      }

      var permissionIds = await iamDbContext.Permissions
         .Where(permission => requiredPermissionCodes.Contains(permission.Code))
         .Select(permission => permission.Id)
         .ToListAsync();

      foreach (var permissionId in permissionIds)
      {
         var rolePermissionExists = await iamDbContext.RolePermissions.AnyAsync(rolePermission =>
            rolePermission.RoleId == userRoleId &&
            rolePermission.PermissionId == permissionId);

         if (!rolePermissionExists)
         {
            iamDbContext.RolePermissions.Add(new RolePermission(userRoleId, permissionId));
         }
      }

      await iamDbContext.SaveChangesAsync();
   }

   private static async Task EnsureRoleDeletePermissionAsync(IamDbContext iamDbContext, SharedDbContext sharedDbContext)
   {
      var roleDeletePermission = await iamDbContext.Permissions
         .FirstOrDefaultAsync(permission => permission.Code == IamPermission.Roles.Delete);

      if (roleDeletePermission == null)
      {
         roleDeletePermission = CreatePermission(IamPermission.Roles.Delete);
         iamDbContext.Permissions.Add(roleDeletePermission);
         await iamDbContext.SaveChangesAsync();
      }
      else if (!roleDeletePermission.IsActive)
      {
         roleDeletePermission.Update(
            roleDeletePermission.Module,
            roleDeletePermission.Resource,
            roleDeletePermission.Action,
            roleDeletePermission.Title,
            roleDeletePermission.Description,
            isActive: true);
         iamDbContext.Permissions.Update(roleDeletePermission);
         await iamDbContext.SaveChangesAsync();
      }

      var organizationAdminRoleId = await sharedDbContext.Parameters
         .Where(parameter => parameter.Key == IamParam.Role.DefaultRoleIdForNewOrganization)
         .Select(parameter => parameter.Value)
         .FirstOrDefaultAsync();

      var roleQuery = iamDbContext.Roles
         .Include(role => role.RolePermissions)
         .AsQueryable();

      Role? organizationAdminRole = null;

      if (Guid.TryParse(organizationAdminRoleId, out var roleId))
      {
         organizationAdminRole = await roleQuery.FirstOrDefaultAsync(role => role.Id == roleId);
      }

      organizationAdminRole ??= await roleQuery.FirstOrDefaultAsync(role => role.Name == "Organization Admin" && role.OrganizationId == null);

      var organizationDeletePermissionId = await iamDbContext.Permissions
         .Where(permission => permission.Code == IamPermission.Organizations.Delete)
         .Select(permission => permission.Id)
         .FirstOrDefaultAsync();

      var adminRoleIds = await iamDbContext.RolePermissions
         .Where(rolePermission => rolePermission.PermissionId == organizationDeletePermissionId)
         .Select(rolePermission => rolePermission.RoleId)
         .ToListAsync();

      if (organizationAdminRole != null)
      {
         adminRoleIds.Add(organizationAdminRole.Id);
      }

      foreach (var adminRoleId in adminRoleIds.Distinct())
      {
         var rolePermissionExists = await iamDbContext.RolePermissions.AnyAsync(rolePermission =>
            rolePermission.RoleId == adminRoleId &&
            rolePermission.PermissionId == roleDeletePermission.Id);

         if (!rolePermissionExists)
         {
            iamDbContext.RolePermissions.Add(new RolePermission(adminRoleId, roleDeletePermission.Id));
         }
      }

      await iamDbContext.SaveChangesAsync();
   }

   private static List<Permission> CreateIamPermissions()
   {
      return typeof(IamPermission)
         .GetNestedTypes()
         .SelectMany(type => type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
         .Where(field => field.IsLiteral && field.FieldType == typeof(string))
         .Select(field => (string)field.GetRawConstantValue()!)
         .Append(IamPermission.Roles.Delete)
         .Distinct(StringComparer.OrdinalIgnoreCase)
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
