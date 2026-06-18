using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;

namespace IAM.Domain.QueryRepositories;

public interface IPermissionQueryRepository
{
   Task<PermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetByParams(PermissionSearchRequest request, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetByAvailablePermissionsRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default); 
   Task<PermissionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
   Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
   Task<bool> CodeExistsAsync(string code, Guid excludedId, CancellationToken cancellationToken = default);
}
