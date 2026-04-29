using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IPermissionService
{
   Task<Result<PermissionDto>> CreateAsync(PermissionCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, PermissionUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result<IEnumerable<PermissionDto>>> GetAllAsync(PermissionSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<PermissionDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

   Task<Result> AssignToRoleAsync(RolePermissionAssignRequest request, CancellationToken cancellationToken = default);
}
