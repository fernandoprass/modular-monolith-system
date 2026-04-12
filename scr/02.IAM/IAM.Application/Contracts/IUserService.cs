using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IUserService
{
   Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<UserLiteDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
   Task<Result<UserDto>> CreateUserAsync(UserCreateRequest request, bool customerExists, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdatePasswordAsync(Guid id, UserUpdatePasswordRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> UpdateLastLoginAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> ValidateUserForNewCustomerAsync(CustomerUserCreateRequest request, CancellationToken cancellationToken = default);
}