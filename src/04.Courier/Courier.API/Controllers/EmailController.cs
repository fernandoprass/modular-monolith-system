using Asp.Versioning;
using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Authorization;

namespace Courier.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/emails")]
[Authorize]
public class EmailController(IEmailService emailService) : BaseController
{
   private readonly IEmailService _emailService = emailService;

   [HttpGet("")]
   [Authorize]
   [RequirePermission(CourierPermission.Emails.List)]
   public async Task<IActionResult> GetByParams([FromBody] EmailSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _emailService.GetAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(CourierPermission.Emails.View)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var result = await _emailService.GetByIdAsync(id, cancellationToken);
      return result.HasError ? NotFound(result) : Ok(result);
   }

   [HttpPost("")]
   [Authorize]
   [RequirePermission(CourierPermission.Emails.Create)]
   public async Task<IActionResult> Create([FromBody] EmailCreateRequest request, CancellationToken cancellationToken)
   {
      var result = await _emailService.CreateAsync(request, cancellationToken);
      return result.HasError ? BadRequest(result) : Created(string.Empty, result);
   }
}
