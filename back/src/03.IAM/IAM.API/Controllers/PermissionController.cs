using Asp.Versioning;
using Shared.Infrastructure.Authorization;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/permissions")]
[Authorize]
public class PermissionController(
   IPermissionService permissionService,
   IUserContext userContext) : BaseController(userContext)
{
   private readonly IPermissionService _permissionService = permissionService;

   [HttpGet]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Read)]
   public async Task<IActionResult> GetByParams([FromQuery] PermissionSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.GetByParams(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Permissions.Write)]
   public async Task<IActionResult> Update(Guid id, [FromBody] PermissionUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _permissionService.UpdateAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

}

