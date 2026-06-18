using Asp.Versioning;
using IAM.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Enums;
using Shared.Infrastructure.Authorization;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/parameters")]
public class ParameterController(IParameterService parameterService) : BaseController
{
   private readonly IParameterService _parameterService = parameterService;

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Parameters.Read)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var parameter = await _parameterService.GetByIdAsync(id, cancellationToken);
      return OkOrNotFound(parameter);
   }

   [HttpGet]
   [Authorize]
   [RequirePermission(IamPermission.Parameters.Read)]
   public async Task<IActionResult> GetByParams([FromQuery] ParameterSearchRequest request, CancellationToken cancellationToken)
   {
      var parameters = await _parameterService.GetAsync(request, cancellationToken);
      return OkOrNotFound(parameters);
   }

   [HttpGet("my-organization")]
   [Authorize]
   [RequirePermission(IamPermission.OrganizationProfile.Parameters)]
   public async Task<IActionResult> GetOrganizationParameters(CancellationToken cancellationToken)
   {
      var parameters = await _parameterService.GetOwnerIdAsync(ParameterOverrideType.Organization, cancellationToken);
      return OkOrNotFound(parameters);
   }

   [HttpGet("me")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.Parameters)]
   public async Task<IActionResult> GetUserParameters(ParameterOverrideType overrideType, CancellationToken cancellationToken)
   {
      var parameters = await _parameterService.GetOwnerIdAsync(ParameterOverrideType.User, cancellationToken);
      return OkOrNotFound(parameters);
   }

   [HttpGet("value")]
   [Authorize]
   [RequirePermission(IamPermission.Parameters.Read)]
   public async Task<IActionResult> GetValue([FromQuery] string key, CancellationToken cancellationToken)
   {
      var parameter = await _parameterService.GetValueAsync(key, cancellationToken);
      return OkOrNotFound(parameter);
   }


   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Parameters.Write)]
   public async Task<IActionResult> Update(Guid id, [FromBody] ParameterUpdateRequest request, CancellationToken cancellationToken)
   {
      var response = await _parameterService.UpdateAsync(id, request, cancellationToken);

      return OkOrNotFound(response);
   }


   [HttpPut("{id:guid}/override")]
   [Authorize]
   [RequirePermission(IamPermission.Parameters.Override)]
   public async Task<IActionResult> SaveOverride(Guid id, ParameterOwnerUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _parameterService.SaveOverrideValueAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}/override")]
   [Authorize]
   [RequirePermission(IamPermission.Parameters.Override)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _parameterService.DeleteOverrideValueAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }
}

