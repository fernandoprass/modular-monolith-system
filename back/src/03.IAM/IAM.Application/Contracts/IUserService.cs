using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Myce.Response;
using Shared.Domain.DTOs.Responses;

namespace IAM.Application.Contracts;

public interface IUserService
{
   Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<UserPasswordDto?> GetByEmailWithPasswordAsync(string email, CancellationToken cancellationToken = default);
   Task<PagedResultDto<UserLiteDto>> GetAsync(UserSearchRequest request, CancellationToken cancellationToken = default);
   Task<IEnumerable<UserLookupDto>> GetLookupAsync(UserLookupRequest request, CancellationToken cancellationToken = default);
   Task<Result<UserDto>> CreateUserAsync(UserCreateRequest request, bool organizationExists, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> DeleteMeAsync(CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateMeAsync(UserUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateOrganizationAdminAsync(Guid id, UserUpdateOrganizationAdminRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdatePasswordAsync(UserUpdatePasswordRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateLastLoginAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> UpdateFailedLoginAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> ValidateUserForNewOrganizationAsync(OrganizationUserCreateRequest request, CancellationToken cancellationToken = default);
}
