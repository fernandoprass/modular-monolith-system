using Myce.Response;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;

namespace Sentinel.Application.Contracts;

public interface ISentinelLogService
{
   Task<Result<PagedResultDto<AuditLogDto>>> GetAuditLogsByParamsAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<PagedResultDto<SystemLogDto>>> GetSystemLogsByParamsAsync(SystemLogSearchRequest request, CancellationToken cancellationToken = default);
}
