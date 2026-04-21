using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IRegisterOrchestrator
{
   Task<Result<CustomerDto>> RegisterCustomerAsync(CustomerCreateRequest customerCreate, CancellationToken cancellationToken = default);
   Task<Result<UserDto>> RegisterUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default);
}
