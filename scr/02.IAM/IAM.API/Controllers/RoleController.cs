using Asp.Versioning;
using IAM.API.Middlewares;
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

   [HttpGet]
   [RequirePermission(IamPermission.Roles.List)]
   public async Task<IActionResult> GetAll(string name, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetAllAsync(name, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost]
   [RequirePermission(IamPermission.Roles.Create)]
   public async Task<IActionResult> Create([FromBody] RoleCreateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.CreateAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   [RequirePermission(IamPermission.Roles.Update)]
   public async Task<IActionResult> Update(Guid id, [FromBody] RoleUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.UpdateAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost("assign")]
   [RequirePermission(IamPermission.Roles.Assign)]
   public async Task<IActionResult> AssignToUser([FromBody] RoleAssignRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.AssignToUserAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("user/{userId:guid}/permissions")]
   [RequirePermission(IamPermission.Roles.ViewPermissions)]
   public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetUserPermissionsAsync(userId, cancellationToken);
      return OkOrNotFound(result);
   }
}
