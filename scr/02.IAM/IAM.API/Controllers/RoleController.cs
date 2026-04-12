using Asp.Versioning;
using IAM.Application.Contracts;
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
   public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
   {
      var result = await _roleService.GetAllAsync(cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost]
   public async Task<IActionResult> Create([FromBody] RoleCreateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.CreateAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   public async Task<IActionResult> Update(Guid id, [FromBody] RoleUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.UpdateAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPost("assign")]
   public async Task<IActionResult> AssignToUser([FromBody] RoleAssignRequest request, CancellationToken cancellationToken)
   {
      var result = await _roleService.AssignToUserAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("user/{userId:guid}/permissions")]
   public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
   {
      var result = await _roleService.GetUserPermissionsAsync(userId, cancellationToken);
      return OkOrNotFound(result);
   }
}
