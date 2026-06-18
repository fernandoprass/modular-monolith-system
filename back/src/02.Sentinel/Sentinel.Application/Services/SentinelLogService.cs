using Myce.Response;
using Sentinel.Application.Contracts;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.QueryRepositories;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Messages;

namespace Sentinel.Application.Services;

public class SentinelLogService(
   ISentinelLogQueryRepository sentinelLogQueryRepository,
   IUserContext userContext) : ISentinelLogService
{
   private readonly ISentinelLogQueryRepository _sentinelLogQueryRepository = sentinelLogQueryRepository;
   private readonly IUserContext _userContext = userContext;

   public async Task<Result<AuditLogDto>> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var auditLog = await _sentinelLogQueryRepository.GetAuditLogByIdAsync(id, _userContext, cancellationToken);

      if (auditLog == null)
      {
         return Result<AuditLogDto>.Failure(new NotFoundError());
      }

      return Result<AuditLogDto>.Success(auditLog);
   }

   public async Task<Result<PagedResultDto<AuditLogLiteDto>>> GetAuditLogsByParamsAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default)
   {
      var auditLogs = await _sentinelLogQueryRepository.GetAuditLogsByParamsAsync(request, _userContext, cancellationToken);
      return Result<PagedResultDto<AuditLogLiteDto>>.Success(auditLogs);
   }

   public async Task<Result<SystemLogDto>> GetSystemLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var systemLog = await _sentinelLogQueryRepository.GetSystemLogByIdAsync(id, _userContext, cancellationToken);

      if (systemLog == null)
      {
         return Result<SystemLogDto>.Failure(new NotFoundError());
      }

      return Result<SystemLogDto>.Success(systemLog);
   }

   public async Task<Result<PagedResultDto<SystemLogLiteDto>>> GetSystemLogsByParamsAsync(SystemLogSearchRequest request, CancellationToken cancellationToken = default)
   {
      var systemLogs = await _sentinelLogQueryRepository.GetSystemLogsByParamsAsync(request, _userContext, cancellationToken);
      return Result<PagedResultDto<SystemLogLiteDto>>.Success(systemLogs);
   }
}
