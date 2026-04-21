using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;

namespace IAM.Domain.QueryRepositories;

public interface IUserQueryRepository
{
   Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Guid> GetIdByEmailAsync(string email, CancellationToken cancellationToken = default);
   Task<IEnumerable<UserLiteDto>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
   Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
   Task<UserPasswordDto?> GetByEmailWithPasswordAsync(string email, CancellationToken cancellationToken = default);
}