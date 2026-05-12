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
   public async Task<IActionResult> GetAuditLogsByParams(AuditLogSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _sentinelLogService.GetAuditLogsByParamsAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("system")]
   [Authorize]
   [RequirePermission(SentinelPermission.SystemLogs.List)]
   public async Task<IActionResult> GetSystemLogsByParams(SystemLogSearchRequest request, CancellationToken cancellationToken)
   {
      var result = await _sentinelLogService.GetSystemLogsByParamsAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }
}
