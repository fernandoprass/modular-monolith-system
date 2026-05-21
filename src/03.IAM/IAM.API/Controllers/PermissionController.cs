using Asp.Versioning;
using Shared.Infrastructure.Authorization;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/permissions")]
[Authorize]
public class PermissionController(IPermissionService permissionService) : BaseController
{
   private readonly IPermissionService _permissionService = permissionService;

   [HttpGet]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.List)]
   public async Task<IActionResult> GetByParams(PermissionSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.GetByParams(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Update)]
   public async Task<IActionResult> Update(Guid id, [FromBody] PermissionUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.UpdateAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost("assign")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Assign)]
   public async Task<IActionResult> AssignToRole([FromBody] RolePermissionAssignRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.AssignToRoleAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("unassign")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Assign)]
   public async Task<IActionResult> UnassignFromRole([FromBody] RolePermissionUnassignRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.UnassignFromRoleAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }
}

