using Asp.Versioning;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Authorization;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/roles")]
[Authorize]
public class RoleController(
   IRoleService roleService,
   IPermissionService permissionService) : BaseController
{
   private readonly IRoleService _roleService = roleService;
   private readonly IPermissionService _permissionService = permissionService;

   [HttpGet("")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Read)]
   public async Task<IActionResult> Get([FromQuery] RoleSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Read)]
   public async Task<IActionResult> GetByRoleId(Guid id, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetByIdAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Write)]
   public async Task<IActionResult> Create([FromBody] RoleCreateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.CreateAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Write)]
   public async Task<IActionResult> Update(Guid id, [FromBody] RoleUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.UpdateAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Roles.Write)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _roleService.DeleteAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost("{id:guid}/permissions")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Assign)]
   public async Task<IActionResult> GetPermission(Guid id, CancellationToken cancellationToken)
   {
      var result = await _permissionService.GetByRoleId(id, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost("{id:guid}/available-permissions")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Assign)]
   public async Task<IActionResult> GetAvailablePermission(Guid id, CancellationToken cancellationToken)
   {
      var result = await _permissionService.GetAvailablePermissionByRoleIdAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost("permissions/assign")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Assign)]
   public async Task<IActionResult> AssignPermission([FromBody] RolePermissionAssignRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.AssignToRoleAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("permissions/unassign")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Assign)]
   public async Task<IActionResult> UnassignPermission([FromBody] RolePermissionUnassignRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.UnassignFromRoleAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }
}

