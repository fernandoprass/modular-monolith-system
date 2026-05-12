using Myce.Response;
using Sentinel.Application.Contracts;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.QueryRepositories;
using Shared.Application.Contracts;

namespace Sentinel.Application.Services;

public class SentinelLogService(
   ISentinelLogQueryRepository sentinelLogQueryRepository,
   IUserContext userContext) : ISentinelLogService
{
   private readonly ISentinelLogQueryRepository _sentinelLogQueryRepository = sentinelLogQueryRepository;
   private readonly IUserContext _userContext = userContext;

   public async Task<Result<PagedResultDto<AuditLogDto>>> GetAuditLogsByParamsAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default)
   {
      var auditLogs = await _sentinelLogQueryRepository.GetAuditLogsByParamsAsync(request, _userContext, cancellationToken);
      return Result<PagedResultDto<AuditLogDto>>.Success(auditLogs);
   }

   public async Task<Result<PagedResultDto<SystemLogDto>>> GetSystemLogsByParamsAsync(SystemLogSearchRequest request, CancellationToken cancellationToken = default)
   {
      var systemLogs = await _sentinelLogQueryRepository.GetSystemLogsByParamsAsync(request, _userContext, cancellationToken);
      return Result<PagedResultDto<SystemLogDto>>.Success(systemLogs);
   }
}
