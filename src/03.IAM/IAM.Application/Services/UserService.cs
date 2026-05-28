using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Isopoh.Cryptography.Argon2;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace IAM.Application.Services;

public class UserService(
    IIamUnitOfWork iamUnitOfWork,
    IParameterService parameterService,
    IRoleService roleService,
    IUserContext userContext,
    IUserValidator userValidator,
    IUserRepository userRepository,
    IUserQueryRepository userQueryRepository,
    IIamEventPublisher eventPublisher) : BaseService(userContext), IUserService
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IParameterService _parameterService = parameterService;
   private readonly IRoleService _roleService = roleService;
   private readonly IUserValidator _userValidator = userValidator;
   private readonly IUserRepository _userRepository = userRepository;
   private readonly IUserQueryRepository _userQueryRepository = userQueryRepository;
   private readonly IIamEventPublisher _eventPublisher = eventPublisher;

   public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _userQueryRepository.GetByIdAsync(id, cancellationToken);
   }

   public async Task<UserPasswordDto?> GetByEmailWithPasswordAsync(string email, CancellationToken cancellationToken = default)
   {
      return await _userQueryRepository.GetByEmailWithPasswordAsync(email, cancellationToken);
   }

   public async Task<IEnumerable<UserLiteDto>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
   {
      return await _userQueryRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
   }

   public async Task<Result<UserDto>> CreateUserAsync(UserCreateRequest request,
                                                      bool organizationExists, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(request.OrganizationId, async (ct) =>
      {
         bool emailExists = await EmailExistsAsync(request.Email, ct);

         var validation = _userValidator.ValidateCreate(request, organizationExists, emailExists);
         if (validation.HasError)
         {
            return Result<UserDto>.Failure(validation.Messages);
         }

         var passwordExpiresAt = await GetPasswordExpiresAt(ct);

         var user = User.Create(
             request.Name,
             request.Email,
             Argon2.Hash(request.Password),
             passwordExpiresAt,
             request.OrganizationId);

         await AddDefaultRolesAsync(user, ct);

         await _iamUnitOfWork.Users.AddAsync(user, ct);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         await _eventPublisher.NotifyEmailAsync(
            IamConst.EmailTemplate.UserWelcome,
            user.OrganizationId,
            user.Id,
            user.Email,
            IamConst.Logger.Feature.Users,
            BuildUserTemplateValues(user),
            ct);

         return Result<UserDto>.Success(user.ToUserDto());
      }, cancellationToken);
   }

   private async Task AddDefaultRolesAsync( User user, CancellationToken ct)
   {
      user.AddRole(await _parameterService.GetGuidAsync(IamParam.Role.DefaultRoleIdForNewUser, ct), null);

      var rolesIds = await _roleService.GetDefaultRolesByOrganizationIdAsync(user.OrganizationId, ct);
      if (rolesIds != null)
      {
         foreach (var id in rolesIds)
         {
            user.AddRole(id, null);
         }
      }
   }

   public async Task<Result> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var user = await _userRepository.GetByIdAsync(id, cancellationToken);
      return await ExecuteIfUserOwnsAsync(user?.OrganizationId, async (ct) =>
      {
         var validator = _userValidator.ValidateUpdate(user?.Id, request);
         if (validator.HasError)
         {
            return Result.Failure(validator.Messages);
         }

         user.Update(request.Name, request.IsActive);

         var result = await CommitUpdateAsync(user, ct);

         if (result.IsSuccess)
         {
            await _eventPublisher.NotifyAuditLogAsync(
               IamConst.Logger.Feature.Users,
               IamConst.Logger.Action.Update,
               AuditPrivacyLevel.Medium,
               $"Updated user {user.Id}",
               user.Id,
               request,
               ct);
         }

         return result;
      }, cancellationToken);
   }

   public async Task<Result> UpdatePasswordAsync(Guid id, UserUpdatePasswordRequest request, CancellationToken cancellationToken = default)
   {
      var user = await _userRepository.GetByIdAsync(id, cancellationToken);

      var validator = _userValidator.ValidateUpdatePassword(user, _userContext.UserId, request);
      if (validator.HasError)
      {
         return Result.Failure(validator.Messages);
      }

      var passwordExpiresAt = await GetPasswordExpiresAt(cancellationToken);

      user.UpdatePassword(Argon2.Hash(request.PasswordNew), passwordExpiresAt);

      var result = await CommitUpdateAsync(user, cancellationToken);

      if (result.IsSuccess)
      {
         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Users,
            IamConst.Logger.Action.UpdatePassword,
            AuditPrivacyLevel.High,
            $"Updated user password {user.Id}",
            user.Id,
            new { user.Id, user.Email },
            cancellationToken);

         await _eventPublisher.NotifyEmailAsync(
            IamConst.EmailTemplate.UserPasswordUpdated,
            user.OrganizationId,
            user.Id,
            user.Email,
            IamConst.Logger.Feature.Users,
            BuildUserTemplateValues(user),
            cancellationToken);
      }

      return result;
   }

   private async Task<DateTime> GetPasswordExpiresAt(CancellationToken cancellationToken)
   {
      short numberOfDay = await _parameterService.GetShortIntAsync(IamParam.Security.MaxPasswordAgeInDays, cancellationToken);

      var passwordExpiresAt = DateTime.UtcNow.AddDays(numberOfDay);
      return passwordExpiresAt;
   }

   public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var user = await _userRepository.GetByIdAsync(id, cancellationToken);

      return await ExecuteIfUserOwnsAsync(user?.OrganizationId, async (ct) =>
      {
         if (user == null)
         {
            return Result.Failure(new NotFoundError(IamConst.Entity.User));
         }

         await _iamUnitOfWork.Users.DeleteAsync(id, ct);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Users,
            IamConst.Logger.Action.Delete,
            AuditPrivacyLevel.High,
            $"Deleted user {id}",
            id,
            new { user.Id, user.Email },
            ct);

         await _eventPublisher.NotifyEmailAsync(
            IamConst.EmailTemplate.UserDelete,
            user.OrganizationId,
            user.Id,
            user.Email,
            IamConst.Logger.Feature.Users,
            BuildUserTemplateValues(user),
            ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }

   public async Task<Result> UpdateLastLoginAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var user = await _userRepository.GetByIdAsync(id, cancellationToken);

      if (user == null) return Result.Failure(new NotFoundError(IamConst.Entity.User));

      user.RegisterLastSuccessfullyLogin();

      return await CommitUpdateAsync(user, cancellationToken);
   }

   public async Task<Result> UpdateFailedLoginAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var user = await _userRepository.GetByIdAsync(id, cancellationToken);

      if (user == null) return Result.Failure(new NotFoundError(IamConst.Entity.User));

      var wasLocked = user.LockedOutUntil.HasValue;

      int maxFailedAttempts = await _parameterService.GetIntAsync(IamParam.Security.MaxFailedLoginAttempts, cancellationToken);
     
      int lockedOutMinutes = await _parameterService.GetIntAsync(IamParam.Security.LockoutDurationInMins, cancellationToken);
      
      user.RegisterFailedLoginAttempt(maxFailedAttempts, lockedOutMinutes);

      var result = await CommitUpdateAsync(user, cancellationToken);

      if (result.IsSuccess && !wasLocked && user.LockedOutUntil.HasValue)
      {
         await _eventPublisher.NotifyEmailAsync(
            IamConst.EmailTemplate.UserMaxFailedLoginAttempts,
            user.OrganizationId,
            user.Id,
            user.Email,
            IamConst.Logger.Feature.Users,
            BuildUserTemplateValues(user),
            cancellationToken);
      }

      return result;
   }

   public async Task<Result> ValidateUserForNewOrganizationAsync(OrganizationUserCreateRequest request, CancellationToken cancellationToken = default)
   {
      bool emailExists = await EmailExistsAsync(request.Email, cancellationToken);

      return _userValidator.ValidateCreateForNewOrganization(request, emailExists);
   }

   private async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
   {
      var userId = await _userQueryRepository.GetIdByEmailAsync(email, cancellationToken);

      var emailExists = userId != Guid.Empty;
      return emailExists;
   }

   private async Task<Result> CommitUpdateAsync(User user, CancellationToken cancellationToken)
   {
      _iamUnitOfWork.Users.Update(user);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   private static IReadOnlyDictionary<string, string> BuildUserTemplateValues(User user)
   {
      return new Dictionary<string, string>
      {
         ["user.name"] = user.Name,
         ["user.email"] = user.Email
      };
   }
}
