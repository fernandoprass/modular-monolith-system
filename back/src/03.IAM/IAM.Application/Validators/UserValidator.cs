using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Messages;
using Isopoh.Cryptography.Argon2;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Messages;

namespace IAM.Application.Validators;

public class UserValidator : IUserValidator
{
   public UserValidator() { }

   public Result ValidateCreate(UserCreateRequest request, bool organizationExists, bool emailAlreadyExists)
   {
      var validator = new FluentValidator<UserCreateRequest>()
            .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplates.NameRules)
            .RuleFor(x => x.Email).ApplyTemplate(ValidatorTemplates.EmailRules)
            .RuleFor(x => x.Password).ApplyTemplate(ValidatorTemplates.PasswordRules)
            .RuleFor(x => x.OrganizationId).IsRequired()
            .RuleFor(x => x.Language).IsRequired()
            .RuleForValue(LanguageOptions.IsSupported(request.Language)).IsTrue(new InvalidLanguageError(request.Language!))
            .RuleForValue(emailAlreadyExists).IsFalse(new EmailAlreadyExistError(request.Email))
            .RuleForValue(organizationExists).IsTrue(new NotFoundError(IamConst.Entity.Organization));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateCreateForNewOrganization(OrganizationUserCreateRequest request, bool emailAlreadyExists)
   {
      var validator = new FluentValidator<OrganizationUserCreateRequest>()
            .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplates.NameRules)
            .RuleFor(x => x.Email).ApplyTemplate(ValidatorTemplates.EmailRules)
            .RuleFor(x => x.Password).ApplyTemplate(ValidatorTemplates.PasswordRules)
            .RuleForValue(emailAlreadyExists).IsFalse(new EmailAlreadyExistError(request.Email));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(Guid? id, UserUpdateRequest request)
   {
      var validator = new FluentValidator<UserUpdateRequest>()
         .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplates.NameRules)
         .RuleFor(x => x.Language).IsRequired()
         .RuleForValue(LanguageOptions.IsSupported(request.Language)).IsTrue(new InvalidLanguageError(request.Language!))
         .RuleForValue(id).IsNotNull(new NotFoundError(IamConst.Entity.User));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdatePassword(User? user, UserUpdatePasswordRequest request)
   {
      var isOldPasswordCorrect = user != null &&
                                 Argon2.Verify(user.PasswordHash, request.PasswordOld);

      var validator = new FluentValidator<UserUpdatePasswordRequest>()
         .RuleForValue(user).IsNotNull(new NotFoundError(IamConst.Entity.User))
         .RuleFor(x => x.PasswordOld).IsRequired()
         .RuleForValue(isOldPasswordCorrect).IsTrue(new PasswordNotValidError())
         .RuleFor(x => x.PasswordNew).ApplyTemplate(ValidatorTemplates.PasswordRules);

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdateOrganizationAdmin(
      User? user,
      IUserContext userContext,
      UserUpdateOrganizationAdminRequest request)
   {
      var isAdminUser = userContext.IsSystemAdmin || userContext.IsOrganizationAdmin;
      var userBelongsToOperatorOrganization = userContext.IsSystemAdmin || user?.OrganizationId == userContext.UserOwnerId;

      var validator = new FluentValidator<UserUpdateOrganizationAdminRequest>()
         .RuleForValue(user).IsNotNull(new NotFoundError(IamConst.Entity.User))
         .RuleForValue(isAdminUser).IsTrue(new Domain.Messages.UnauthorizedAccessError())
         .RuleForValue(userBelongsToOperatorOrganization).IsTrue(new Domain.Messages.UnauthorizedAccessError());

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

}
