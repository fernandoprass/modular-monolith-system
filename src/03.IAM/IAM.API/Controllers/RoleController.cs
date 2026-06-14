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

