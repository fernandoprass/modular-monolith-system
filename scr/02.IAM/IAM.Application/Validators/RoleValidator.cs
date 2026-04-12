using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Messages.Errors;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Domain.Messages;

namespace IAM.Application.Validators;

public class RoleValidator : IRoleValidator
{
   public Result ValidateCreate(RoleCreateRequest request, bool nameAlreadyExists)
   {
      var validator = new FluentValidator<RoleCreateRequest>()
         .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplate.NameRules)
         .RuleForValue(nameAlreadyExists).IsFalse(new DuplicateRoleNameError(request.Name));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(Guid? id, RoleUpdateRequest request, bool isDefault)
   {
      var validator = new FluentValidator<RoleUpdateRequest>()
         .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplate.NameRules)
         .Custom(id is not null, new NotFoundError(IamConst.Entity.Role))
         .Custom(!isDefault, new CannotUpdateDefaultRoleError());

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateAssign(RoleAssignRequest request, bool userExists, bool allRolesExist)
   {
      var validator = new FluentValidator<RoleAssignRequest>()
         .RuleForValue(userExists).IsTrue(new NotFoundError(IamConst.Entity.User))
         .RuleForValue(allRolesExist).IsTrue(new NotFoundError(IamConst.Entity.Role))
         .RuleFor(x => x.Roles).IsNotNull();

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
