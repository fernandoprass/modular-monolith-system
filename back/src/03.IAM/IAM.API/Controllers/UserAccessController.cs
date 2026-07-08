using Asp.Versioning;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;
using Shared.Infrastructure.Authorization;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/user-access")]
[Authorize]
public class UserAccessController(
   IRoleService roleService,
   IUserContext userContext) : BaseController(userContext)
{
   private readonly IRoleService _roleService = roleService;

   [HttpPost("roles/assign")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Assign)]
   public async Task<IActionResult> AssignRole([FromBody] RoleAssignRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.AssignToUserAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("roles/unassign")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Assign)]
   public async Task<IActionResult> UnassignRole([FromBody] RoleUnassignRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.UnassignFromUserAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("users/{userId:guid}/available-roles")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Assign)]
   public async Task<IActionResult> GetAvailableRoles(Guid userId, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetAvailableRolesByUserIdAsync(userId, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("users/{userId:guid}/permissions")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.ViewAccess)]
   public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetRolePermissionsByUserIdAsync(userId, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("users/{userId:guid}/permission-codes")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.ViewAccess)]
   public async Task<IActionResult> GetUserPermissionCodes(Guid userId, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetPermissionCodesByUserIdAsync(userId, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("users/{userId:guid}/roles")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.ViewAccess)]
   public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetRolesByUserIdAsync(userId, cancellationToken);
      return OkOrNotFound(result);
   }
}
