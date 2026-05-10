using IAM.Domain.Entities;

namespace IAM.Domain.Repositories;

public interface IUserRepository
{
   Task AddAsync(User user, CancellationToken cancellationToken = default);
   void Update(User user);
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
   Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}