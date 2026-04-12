using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Requests;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/parameters")]
public class ParameterController(IParameterService parameterService) : BaseController
{
   private readonly IParameterService _parameterService = parameterService;

   [HttpGet("{id:guid}")]
   [Authorize]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var parameter = await _parameterService.GetByIdAsync(id, cancellationToken);
      return OkOrNotFound(parameter);
   }

   [HttpGet]
   [Authorize]
   public async Task<IActionResult> Get(ParameterSearchRequest request, CancellationToken cancellationToken)
   {
      var parameters = await _parameterService.GetAsync(request, cancellationToken);
      return OkOrNotFound(parameters);
   }

   [HttpGet("key/{key}")]
   [Authorize]
   public async Task<IActionResult> GetByKey(string key, CancellationToken cancellationToken)
   {
      var parameter = await _parameterService.GetValueAsync(key, cancellationToken);
      return OkOrNotFound(parameter);
   }

   [HttpPut("{id:guid}/override")]
   [Authorize]
   public async Task<IActionResult> SaveOverride(Guid id, ParameterOwnerUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _parameterService.SaveOverrideValueAsync(id, request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}/override")]
   [Authorize]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _parameterService.DeleteOverrideValueAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }
}
