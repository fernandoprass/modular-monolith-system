using Asp.Versioning;
using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Authorization;

namespace Courier.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/email-templates")]
[Authorize]
public class EmailTemplateController(IEmailTemplateService emailTemplateService) : BaseController
{
   private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;

   [HttpGet("")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.List)]
   public async Task<IActionResult> GetByParams([FromQuery] EmailTemplateSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.GetAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.View)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.GetByIdAsync(id, cancellationToken);
      return result.HasError ? NotFound(result) : Ok(result);
   }

   [HttpPost("")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.Create)]
   public async Task<IActionResult> Create([FromBody] EmailTemplateCreateRequest request, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.CreateAsync(request, cancellationToken);
      return result.HasError ? BadRequest(result) : Created(string.Empty, result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.Update)]
   public async Task<IActionResult> Update(Guid id, [FromBody] EmailTemplateUpdateRequest request, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.UpdateAsync(id, request, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.Delete)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.DeleteAsync(id, cancellationToken);
      return result.HasError ? NotFound(result) : Ok(result);
   }

   [HttpPost("{id:guid}/translations")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.Update)]
   public async Task<IActionResult> AddTranslation(Guid id, [FromBody] EmailTemplateTranslationRequest request, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.AddTranslationAsync(id, request, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpPut("{id:guid}/translations/{language}")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.Update)]
   public async Task<IActionResult> UpdateTranslation(Guid id, string language, [FromBody] EmailTemplateTranslationRequest request, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.UpdateTranslationAsync(id, language, request, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpDelete("{id:guid}/translations/{language}")]
   [Authorize]
   [RequirePermission(CourierPermission.EmailTemplates.Update)]
   public async Task<IActionResult> RemoveTranslation(Guid id, string language, CancellationToken cancellationToken)
   {
      var result = await _emailTemplateService.RemoveTranslationAsync(id, language, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }
}
