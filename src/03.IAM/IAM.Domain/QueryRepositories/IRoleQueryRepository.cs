using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.DTOs.Requests;

namespace IAM.Domain.QueryRepositories;

public interface IRoleQueryRepository
{
   Task<int> CountRolesByRoleIdsAsync(IEnumerable<Guid> ids, Guid organizationId, CancellationToken cancellationToken = default);
   Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<Guid>> GetDefaultRolesByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
   Task<IEnumerable<RoleDto>> GetAsync(RoleSearchRequest request, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
   Task<IEnumerable<Permission>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
   Task<bool> NameExistsAsync(string name, Guid? organizationId, CancellationToken cancellationToken = default); 
}
