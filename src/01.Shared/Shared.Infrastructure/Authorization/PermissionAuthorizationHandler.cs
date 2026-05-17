using Microsoft.AspNetCore.Authorization;
using Shared.Application.Contracts;
using Shared.Domain;

namespace Shared.Infrastructure.Authorization;

public class PermissionAuthorizationHandler(IRolePermissionCache permissionService)
   : AuthorizationHandler<RequirePermissionAttribute>
{
   private readonly IRolePermissionCache _permissionService = permissionService;

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

      var isSystemAdmin = user.FindFirst(SharedConst.Security.Claim.IsSystemAdmin)?.Value;
      if (bool.TryParse(isSystemAdmin, out var isSysAdmin) && isSysAdmin)
      {
         context.Succeed(requirement);
         return;
      }

      var roleClaims = user.FindAll(SharedConst.Security.Claim.Role)
         .Select(claim => claim.Value)
         .Where(role => Guid.TryParse(role, out _))
         .Distinct(StringComparer.OrdinalIgnoreCase)
         .ToArray();

      if (roleClaims.Length == 0)
      {
         context.Fail();
         return;
      }

      var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var role in roleClaims)
      {
         var permissions = await _permissionService.GetPermissionsAsync(role);
         userPermissions.UnionWith(permissions);
      }

      if (userPermissions.Contains(requirement.Permission))
      {
         context.Succeed(requirement);
         return;
      }

      context.Fail();
   }
}
