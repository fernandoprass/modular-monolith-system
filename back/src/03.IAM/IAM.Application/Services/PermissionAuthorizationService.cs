using IAM.Application.Contracts;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Shared.Application.Contracts;
using SharedPermissionService = Shared.Application.Contracts.IRolePermissionCache;

namespace IAM.Application.Services;

public class PermissionAuthorizationService(
   IUserContext userContext,
   SharedPermissionService permissionService) : IPermissionAuthorizationService
{
   private readonly IUserContext _userContext = userContext;
   private readonly SharedPermissionService _permissionService = permissionService;

   public async Task<PermissionCheckResponse> CheckPermissionAsync(PermissionCheckRequest request, CancellationToken cancellationToken = default)
   {
      var roleIds = _userContext.Roles
         .Select(role => Guid.TryParse(role, out var roleId) ? roleId : (Guid?)null)
         .Where(roleId => roleId.HasValue)
         .Select(roleId => roleId!.Value)
         .Distinct()
         .ToArray();

      if (string.IsNullOrWhiteSpace(request.Permission) || roleIds.Length == 0)
      {
         return new PermissionCheckResponse(false);
      }

      var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var roleId in roleIds)
      {
         var permissions = await _permissionService.GetPermissionsAsync(roleId.ToString(), cancellationToken);
         userPermissions.UnionWith(permissions);
      }

      return new PermissionCheckResponse(userPermissions.Contains(request.Permission));
   }
}
