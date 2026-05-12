using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Shared.Application.Contracts;

namespace Sentinel.Domain.QueryRepositories;

public interface ISentinelLogQueryRepository
{
   Task<PagedResultDto<AuditLogDto>> GetAuditLogsByParamsAsync(AuditLogSearchRequest request, IUserContext userContext, CancellationToken cancellationToken = default);
   Task<PagedResultDto<SystemLogDto>> GetSystemLogsByParamsAsync(SystemLogSearchRequest request, IUserContext userContext, CancellationToken cancellationToken = default);
}
