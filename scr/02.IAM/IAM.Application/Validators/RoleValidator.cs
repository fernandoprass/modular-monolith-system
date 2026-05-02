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

   public Result ValidateAssign(RoleAssignRequest request, bool userExists, bool allRolesAvailable)
   {
      var isThereRoleExpiringInThePass = request.Roles.Any(r => r.ExpiresAt != null && r.ExpiresAt < DateTime.UtcNow);
      var validator = new FluentValidator<RoleAssignRequest>()
         .RuleForValue(userExists).IsTrue(new NotFoundError(IamConst.Entity.User))
         .RuleFor(x => x.Roles).IsNotNull().Stop().HasItems<RoleAssignRequest, IEnumerable<RoleAssignmentDto>, RoleAssignmentDto>()
         .RuleForValue(allRolesAvailable).IsTrue(new RolesCannotBeAssignedError())
         .RuleForValue(isThereRoleExpiringInThePass).IsFalse(new RolesInvalidExpirationError());

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUnassign(RoleUnassignRequest request, bool userExists, bool userHasAllRoles)
   {
      var validator = new FluentValidator<RoleUnassignRequest>()
         .RuleForValue(userExists).IsTrue(new NotFoundError(IamConst.Entity.User))
         .RuleFor(x => x.RoleIds).IsNotNull().Stop().HasItems<RoleUnassignRequest, IEnumerable<Guid>, Guid>()
         .RuleForValue(userHasAllRoles).IsTrue(new RolesCannotBeUnassignedError());

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
