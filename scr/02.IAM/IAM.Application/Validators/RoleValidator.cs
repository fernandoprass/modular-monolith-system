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
         .RuleForValue(nameAlreadyExists).IsFalse(new RoleDuplicateNameError(request.Name))
         .If(r => r.OrganizationId is not null, then => then.RuleFor(r => r.OrganizationId).IsEqualTo(userContext.UserOwnerId, new OrganizationForbiddenError()));

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

   public Result ValidateAssign(RoleAssignRequest request, bool userExists, bool allRolesAvailable)
   {
      var validator = new FluentValidator<RoleAssignRequest>()
         .RuleForValue(userExists).IsTrue(new NotFoundError(IamConst.Entity.User))
         .RuleForValue(allRolesAvailable).IsTrue(new RolesCannotBeAssignedError())
         .RuleFor(x => x.Roles.Select(r => r.RoleId)).HasItems().HasNoDuplicates()
         .RuleFor(x => x.Roles.Select(r => r.ExpiresAt))
            .All<RoleAssignRequest, IEnumerable<DateTime?>, DateTime?>(expireDate => expireDate == null || expireDate >= DateTime.UtcNow, new RolesInvalidExpirationError());

      var isValid = validator.Validate(request, shortCircuitMode: true);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUnassign(RoleUnassignRequest request, bool userExists, bool userHasAllRoles)
   {
      var validator = new FluentValidator<RoleUnassignRequest>()
         .RuleForValue(userExists).IsTrue(new NotFoundError(IamConst.Entity.User))
         .RuleForValue(userHasAllRoles).IsTrue(new RolesCannotBeUnassignedError())
         .RuleFor(x => x.RoleIds).HasItems().HasNoDuplicates();

      var isValid = validator.Validate(request, shortCircuitMode: true);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
