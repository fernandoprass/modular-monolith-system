using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IAuthService
{
   Task<Result<LoginResponse?>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default);
}
