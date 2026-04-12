using IAM.Domain.Entities;

namespace IAM.Domain.Repositories;

public interface ICustomerRepository
{
   Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
   void Update(Customer customer);
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Customer?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
   Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}