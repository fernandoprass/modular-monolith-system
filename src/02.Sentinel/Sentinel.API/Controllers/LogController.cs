using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Contracts;
using Sentinel.Domain;
using Sentinel.Domain.DTOs.Requests;
using Shared.Infrastructure.Authorization;

namespace Sentinel.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/sentinel/logs")]
[Authorize]
public class LogController(ISentinelLogService sentinelLogService) : BaseController
{
   private readonly ISentinelLogService _sentinelLogService = sentinelLogService;

   [HttpGet("audit")]
   [Authorize]
   [RequirePermission(SentinelPermission.AuditLogs.List)]
   public async Task<IActionResult> GetAuditLogsByParams([FromQuery] AuditLogSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _sentinelLogService.GetAuditLogsByParamsAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("audit/{id:guid}")]
   [Authorize]
   [RequirePermission(SentinelPermission.AuditLogs.View)]
   public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken cancellationToken)
   {
      var result = await _sentinelLogService.GetAuditLogByIdAsync(id, cancellationToken);
      return result.HasError ? NotFound(result) : Ok(result);
   }

   [HttpGet("system")]
   [Authorize]
   [RequirePermission(SentinelPermission.SystemLogs.List)]
   public async Task<IActionResult> GetSystemLogsByParams([FromQuery] SystemLogSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _sentinelLogService.GetSystemLogsByParamsAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("system/{id:guid}")]
   [Authorize]
   [RequirePermission(SentinelPermission.SystemLogs.View)]
   public async Task<IActionResult> GetSystemLogById(Guid id, CancellationToken cancellationToken)
   {
      var result = await _sentinelLogService.GetSystemLogByIdAsync(id, cancellationToken);
      return result.HasError ? NotFound(result) : Ok(result);
   }
}
