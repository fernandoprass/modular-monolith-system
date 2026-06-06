using Asp.Versioning;
using Shared.Infrastructure.Authorization;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/roles")]
[Authorize]
public class RoleController(IRoleService roleService) : BaseController
{
   private readonly IRoleService _roleService = roleService;

   [HttpGet("")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.List)]
   public async Task<IActionResult> Get([FromQuery] RoleSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Create)]
   public async Task<IActionResult> Create([FromBody] RoleCreateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.CreateAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Update)]
   public async Task<IActionResult> Update(Guid id, [FromBody] RoleUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.UpdateAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Delete)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _roleService.DeleteAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost("assign")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Assign)]
   public async Task<IActionResult> AssignToUser([FromBody] RoleAssignRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.AssignToUserAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("unassign")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Assign)]
   public async Task<IActionResult> UnassignFromUser([FromBody] RoleUnassignRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.UnassignFromUserAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("user/{userId:guid}/permissions")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.ViewPermissions)]
   public async Task<IActionResult> GetRoleUserPermissions(Guid userId, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetRolePermissionsByUserIdAsync(userId, cancellationToken);
      return OkOrNotFound(result);
   }
}

