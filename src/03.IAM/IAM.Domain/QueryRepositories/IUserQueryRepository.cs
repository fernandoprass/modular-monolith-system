using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using Shared.Domain.DTOs.Responses;

namespace IAM.Domain.QueryRepositories;

public interface IUserQueryRepository
{
   Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Guid> GetIdByEmailAsync(string email, CancellationToken cancellationToken = default);
   Task<PagedResultDto<UserLiteDto>> GetAsync(UserSearchRequest request, CancellationToken cancellationToken = default);
   Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
   Task<UserPasswordDto?> GetByEmailWithPasswordAsync(string email, CancellationToken cancellationToken = default);
}