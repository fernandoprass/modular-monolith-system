using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using Myce.Response;
using Shared.Application.Contracts;

namespace IAM.Domain.QueryRepositories;

public interface IRoleQueryRepository
{
   Task<int> CountRolesByRoleIdsAsync(IEnumerable<Guid> ids, Guid OrganizationId, IUserContext userContext, CancellationToken cancellationToken = default);
   Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<Guid>> GetDefaultRolesByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
   Task<IEnumerable<RoleDto>> GetByNameAsync(string? name, Guid? organizationId, IUserContext userContext, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
   Task<IEnumerable<Permission>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
   Task<bool> NameExistsAsync(string name, Guid? organizationId, IUserContext userContext, CancellationToken cancellationToken = default); 
}
