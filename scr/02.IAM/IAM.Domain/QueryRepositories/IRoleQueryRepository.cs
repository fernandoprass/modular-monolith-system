using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;

namespace IAM.Domain.QueryRepositories;

public interface IRoleQueryRepository
{
   Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<RoleDto>> GetAllAsync(string name, Guid organizationId, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
   Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
   Task<bool> NameExistsAsync(string name, Guid? organizationId, CancellationToken cancellationToken = default);
}
