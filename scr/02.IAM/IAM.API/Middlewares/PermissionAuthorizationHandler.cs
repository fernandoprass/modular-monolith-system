using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.QueryRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace IAM.API.Middlewares;

public class PermissionAuthorizationHandler(
    IServiceProvider serviceProvider,
    IMemoryCache cache) : AuthorizationHandler<RequirePermissionAttribute>
{
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly IMemoryCache _cache = cache;

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

      var userPermissions = new HashSet<string>();

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
      var cacheKey = $"role_permissions_{roleId}";

      return await _cache.GetOrCreateAsync(cacheKey, async entry =>
      {
         entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

         // Create scope to resolve scoped service
         using var scope = _serviceProvider.CreateScope();
         var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();

         var permissions = await roleService.GetPermissionsByRoleIdAsync(roleId);
         return permissions.Select(p => p.Code).ToList();
      }) ?? Enumerable.Empty<string>();
   }
}