using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IRegisterOrchestrator
{
   Task<Result<OrganizationDto>> RegisterOrganizationAsync(OrganizationCreateRequest organizationCreate, CancellationToken cancellationToken = default);
   Task<Result<UserDto>> RegisterUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteOrganizationAsync(Guid id, CancellationToken cancellationToken = default);
}
