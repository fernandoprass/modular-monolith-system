using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;

using Myce.Response;

namespace IAM.Application.Contracts;

public interface IOrganizationService
{
   Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   string GetRandomCode();
   Task<IEnumerable<OrganizationDto>> GetByNameAsync(string? name, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, OrganizationUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateCodeAsync(Guid id, OrganizationUpdateCodeRequest request, CancellationToken cancellationToken = default);
   Task<Result> ValidateCreateOrganizationAsync(OrganizationCreateRequest request, CancellationToken cancellationToken = default);
}
