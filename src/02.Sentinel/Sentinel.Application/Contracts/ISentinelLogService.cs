using Myce.Response;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Shared.Domain.DTOs.Responses;

namespace Sentinel.Application.Contracts;

public interface ISentinelLogService
{
   Task<Result<PagedResultDto<AuditLogLiteDto>>> GetAuditLogsByParamsAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<PagedResultDto<SystemLogLiteDto>>> GetSystemLogsByParamsAsync(SystemLogSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<AuditLogDto>> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result<SystemLogDto>> GetSystemLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
