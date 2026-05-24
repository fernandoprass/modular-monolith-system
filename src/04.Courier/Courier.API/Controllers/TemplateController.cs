using Asp.Versioning;
using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Authorization;

namespace Courier.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/templates")]
[Authorize]
public class TemplateController(ITemplateService templateService) : BaseController
{
   private readonly ITemplateService _templateService = templateService;

   [HttpGet("")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.List)]
   public async Task<IActionResult> GetByParams([FromBody] TemplateSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _templateService.GetAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.View)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var result = await _templateService.GetByIdAsync(id, cancellationToken);
      return result.HasError ? NotFound(result) : Ok(result);
   }

   [HttpPost("")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.Create)]
   public async Task<IActionResult> Create([FromBody] TemplateCreateRequest request, CancellationToken cancellationToken)
   {
      var result = await _templateService.CreateAsync(request, cancellationToken);
      return result.HasError ? BadRequest(result) : Created(string.Empty, result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.Update)]
   public async Task<IActionResult> Update(Guid id, [FromBody] TemplateUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _templateService.UpdateAsync(id, request, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.Delete)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _templateService.DeleteAsync(id, cancellationToken);
      return result.HasError ? NotFound(result) : Ok(result);
   }

   [HttpPost("{id:guid}/email-translations")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.Update)]
   public async Task<IActionResult> AddEmailTranslation(Guid id, [FromBody] TemplateEmailTranslationRequest request, CancellationToken cancellationToken)
   {
      var result = await _templateService.AddEmailTranslationAsync(id, request, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpPut("{id:guid}/email-translations/{language}")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.Update)]
   public async Task<IActionResult> UpdateEmailTranslation(Guid id, string language, [FromBody] TemplateEmailTranslationRequest request, CancellationToken cancellationToken)
   {
      var result = await _templateService.UpdateEmailTranslationAsync(id, language, request, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpDelete("{id:guid}/translations/{language}")]
   [Authorize]
   [RequirePermission(CourierPermission.Templates.Update)]
   public async Task<IActionResult> RemoveTranslation(Guid id, string language, CancellationToken cancellationToken)
   {
      var result = await _templateService.RemoveTranslationAsync(id, language, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }
}
