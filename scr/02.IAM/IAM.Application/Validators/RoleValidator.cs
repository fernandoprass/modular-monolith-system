using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Messages;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain.Messages;

namespace IAM.Application.Validators;

public class RoleValidator(IUserContext userContext) : IRoleValidator
{
   public Result ValidateCreate(RoleCreateRequest request, bool nameAlreadyExists)
   {
      var validator = new FluentValidator<RoleCreateRequest>()
         .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplates.NameRules)
         .If(r => r.OrganizationId is not null, then => then.RuleFor(r => r.OrganizationId).IsEqualTo(userContext.UserOwnerId, new OrganizationForbiddenError()))
         .RuleForValue(nameAlreadyExists).IsFalse(new RoleDuplicateNameError(request.Name));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(RoleUpdateRequest request, bool roleExists)
   {
      var validator = new FluentValidator<RoleUpdateRequest>()
         .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplates.NameRules)
         .RuleForValue(roleExists).IsTrue(new NotFoundError(IamConst.Entity.Role));

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
