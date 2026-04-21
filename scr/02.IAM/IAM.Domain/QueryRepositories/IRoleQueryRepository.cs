using IAM.Domain.Entities;

namespace IAM.Domain.QueryRepositories;

public interface IRoleQueryRepository
{
   Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<Role>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken = default);
   Task<bool> NameExistsAsync(string name, Guid? organizationId, CancellationToken cancellationToken = default);
   Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
