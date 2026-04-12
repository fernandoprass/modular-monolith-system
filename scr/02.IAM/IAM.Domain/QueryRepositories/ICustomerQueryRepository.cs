using IAM.Domain.DTOs.Responses;

namespace IAM.Domain.QueryRepositories;

public interface ICustomerQueryRepository
{
   Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<CustomerDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
   Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
   Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}