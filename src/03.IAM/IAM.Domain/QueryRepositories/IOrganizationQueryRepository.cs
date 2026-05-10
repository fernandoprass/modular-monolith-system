using IAM.Domain.DTOs.Responses;

namespace IAM.Domain.QueryRepositories;

public interface IOrganizationQueryRepository
{
   Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<OrganizationDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
   Task<IEnumerable<OrganizationDto>> GetAllAsync(CancellationToken cancellationToken = default);
   Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}