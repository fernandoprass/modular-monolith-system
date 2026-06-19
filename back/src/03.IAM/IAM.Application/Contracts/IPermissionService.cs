using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;
using Shared.Domain.DTOs.Responses;

namespace IAM.Application.Contracts;

public interface IPermissionService
{
   Task<Result<PermissionDto>> CreateAsync(PermissionCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, PermissionUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<PagedResultDto<PermissionDto>> GetByParams(PermissionSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<IEnumerable<PermissionDto>>> GetByRoleId(Guid roleId, CancellationToken cancellationToken = default);
   Task<Result<IEnumerable<PermissionDto>>> GetAvailablePermissionByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
   Task<Result<PermissionDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
   Task<Result> AssignToRoleAsync(RolePermissionAssignRequest request, CancellationToken cancellationToken = default);
   Task<Result> UnassignFromRoleAsync(RolePermissionUnassignRequest request, CancellationToken cancellationToken = default);
}
