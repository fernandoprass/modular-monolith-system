using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Shared.Domain.DTOs.Responses;

namespace IAM.Domain.QueryRepositories;

public interface IOrganizationQueryRepository
{
   Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<PagedResultDto<OrganizationDto>> GetAsync(OrganizationSearchRequest request, CancellationToken cancellationToken = default);
   Task<IEnumerable<OrganizationLookupDto>> GetLookupAsync(OrganizationLookupRequest request, CancellationToken cancellationToken = default);
   Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
