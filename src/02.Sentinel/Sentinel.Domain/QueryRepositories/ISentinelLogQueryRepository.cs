using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;

namespace Sentinel.Domain.QueryRepositories;

public interface ISentinelLogQueryRepository
{
   Task<PagedResultDto<AuditLogLiteDto>> GetAuditLogsByParamsAsync(AuditLogSearchRequest request, IUserContext userContext, CancellationToken cancellationToken = default);
   Task<PagedResultDto<SystemLogLiteDto>> GetSystemLogsByParamsAsync(SystemLogSearchRequest request, IUserContext userContext, CancellationToken cancellationToken = default);
   Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id, IUserContext userContext, CancellationToken cancellationToken = default);
   Task<SystemLogDto?> GetSystemLogByIdAsync(Guid id, IUserContext userContext, CancellationToken cancellationToken = default);
}
