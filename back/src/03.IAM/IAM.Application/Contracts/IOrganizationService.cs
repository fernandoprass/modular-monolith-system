using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;

using Myce.Response;
using Shared.Domain.DTOs.Responses;

namespace IAM.Application.Contracts;

public interface IOrganizationService
{
   Task<Result<OrganizationDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   string GetRandomCode();
   Task<Result<PagedResultDto<OrganizationDto>>> GetAsync(OrganizationSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<IEnumerable<OrganizationLookupDto>>> GetLookupAsync(OrganizationLookupRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, OrganizationUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateCodeAsync(Guid id, OrganizationUpdateCodeRequest request, CancellationToken cancellationToken = default);
   Task<Result> ValidateCreateOrganizationAsync(OrganizationCreateRequest request, CancellationToken cancellationToken = default);
}
