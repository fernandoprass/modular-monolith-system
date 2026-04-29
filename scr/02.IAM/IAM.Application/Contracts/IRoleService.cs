using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IRoleService
{
   Task<Result<RoleDto>> CreateAsync(RoleCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, RoleUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> AssignToUserAsync(RoleAssignRequest request, CancellationToken cancellationToken = default);
   Task<Result<IEnumerable<RoleDto>>> GetByNameAsync(string? name, CancellationToken cancellationToken = default);
   Task<Result<IEnumerable<PermissionDto>>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
