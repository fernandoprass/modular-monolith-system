using IAM.Application.Contracts;
using IAM.Domain;
using Microsoft.AspNetCore.Authorization;

namespace IAM.API.Middlewares;

public class PermissionAuthorizationHandler(
   IServiceProvider serviceProvider,
   IRolePermissionAuthorizationCache rolePermissionAuthorizationCache) : AuthorizationHandler<RequirePermissionAttribute>
{
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly IRolePermissionAuthorizationCache _rolePermissionAuthorizationCache = rolePermissionAuthorizationCache;

   protected override async Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      RequirePermissionAttribute requirement)
   {
      var user = context.User;

      if (!user.Identity?.IsAuthenticated ?? true)
      {
         context.Fail();
         return;
      }

      var isSystemAdmin = user.FindFirst(IamConst.Security.Claim.IsSystemAdmin)?.Value;
      if (bool.TryParse(isSystemAdmin, out var isSysAdmin) && isSysAdmin)
      {
         context.Succeed(requirement);
         return;
      }

      var roleClaims = user.FindAll(IamConst.Security.Claim.Role)
         .Select(c => c.Value)
         .ToList();

      if (!roleClaims.Any())
      {
         context.Fail();
         return;
      }

      var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var roleId in roleClaims)
      {
         if (!Guid.TryParse(roleId, out var parsedRoleId))
            continue;

         var permissions = await GetRolePermissionsAsync(parsedRoleId);
         userPermissions.UnionWith(permissions);
      }

      if (userPermissions.Contains(requirement.Permission))
      {
         context.Succeed(requirement);
      }
      else
      {
         context.Fail();
      }
   }

   private async Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId)
   {
      return await _rolePermissionAuthorizationCache.GetOrCreateAsync(roleId, async () =>
      {
         using var scope = _serviceProvider.CreateScope();
         var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();

         var permissions = await roleService.GetPermissionsByRoleIdAsync(roleId);
         return permissions.Select(p => p.Code).ToList();
      });
   }
}
