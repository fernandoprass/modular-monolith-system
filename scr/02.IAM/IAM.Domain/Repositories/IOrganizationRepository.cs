using IAM.Domain.Entities;

namespace IAM.Domain.Repositories;

public interface IOrganizationRepository
{
   Task AddAsync(Organization organization, CancellationToken cancellationToken = default);
   void Update(Organization organization);
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
   Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}