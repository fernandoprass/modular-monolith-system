using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Messages;
using Isopoh.Cryptography.Argon2;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Domain.Messages;

namespace IAM.Application.Validators;

public class UserValidator : IUserValidator
{
   public UserValidator() { }

   public Result ValidateCreate(UserCreateRequest request, bool organizationExists, bool emailAlreadyExists)
   {
      var validator = new FluentValidator<UserCreateRequest>()
            .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplate.NameRules)
            .RuleFor(x => x.Email).ApplyTemplate(ValidatorTemplate.EmailRules)
            .RuleFor(x => x.Password).ApplyTemplate(ValidatorTemplate.PasswordRules)
            .RuleFor(x => x.OrganizationId).IsRequired()
            .RuleForValue(emailAlreadyExists).IsFalse(new EmailAlreadyExistError(request.Email))
            .RuleForValue(organizationExists).IsTrue(new NotFoundError(IamConst.Entity.Organization));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateCreateForNewOrganization(OrganizationUserCreateRequest request, bool emailAlreadyExists)
   {
      var validator = new FluentValidator<OrganizationUserCreateRequest>()
            .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplate.NameRules)
            .RuleFor(x => x.Email).ApplyTemplate(ValidatorTemplate.EmailRules)
            .RuleFor(x => x.Password).ApplyTemplate(ValidatorTemplate.PasswordRules)
            .RuleForValue(emailAlreadyExists).IsFalse(new EmailAlreadyExistError(request.Email));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(Guid? id, UserUpdateRequest request)
   {
      var validator = new FluentValidator<UserUpdateRequest>()
         .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplate.NameRules)
         .Custom(id is not null, new NotFoundError(IamConst.Entity.User));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdatePassword(User? user, Guid loggedUserId, UserUpdatePasswordRequest request)
   {
      var isOldPasswordCorrect = user != null &&
                                 Argon2.Verify(user.PasswordHash, request.PasswordOld);

      var validator = new FluentValidator<UserUpdatePasswordRequest>()
         .RuleForValue(user).IsNotNull(new NotFoundError(IamConst.Entity.User))
         .RuleForValue(user?.Id).IsEqualTo(loggedUserId, new UnauthorizedAccessError())
         .RuleFor(x => x.PasswordOld).IsRequired()
         .RuleForValue(isOldPasswordCorrect).IsTrue(new PasswordNotValidError())
         .RuleFor(x => x.PasswordNew).ApplyTemplate(ValidatorTemplate.PasswordRules);

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }


}
