using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;

using Myce.Response;

namespace IAM.Application.Contracts;

public interface ICustomerService
{
   Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   string GetRandomCode();
   Task<IEnumerable<CustomerDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, CustomerUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateCodeAsync(Guid id, CustomerUpdateCodeRequest request, CancellationToken cancellationToken = default);
   Task<Result> ValidateCreateCustomerAsync(CustomerCreateRequest request, CancellationToken cancellationToken = default);
}