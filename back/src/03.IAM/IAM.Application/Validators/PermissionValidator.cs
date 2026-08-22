using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Messages;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Domain.Messages;

namespace IAM.Application.Validators;

public class PermissionValidator : IPermissionValidator
{
   private static void MemberCodeTemplate<T>(RuleBuilder<T, string> rb) where T : class
                     => rb.IsRequired().MinLength(3).IsAlphaNumeric();

   public Result ValidateCreate(PermissionCreateRequest request, bool codeAlreadyExists)
   {
      string code = Permission.BuildCode(request.Module, request.Resource, request.Action);

      var validator = new FluentValidator<PermissionCreateRequest>()
         .RuleFor(x => x.Module).ApplyTemplate(MemberCodeTemplate)
         .RuleFor(x => x.Resource).ApplyTemplate(MemberCodeTemplate)
         .RuleFor(x => x.Action).ApplyTemplate(MemberCodeTemplate)
         .RuleForValue(codeAlreadyExists).IsFalse(new PermissionDuplicateError(code));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(PermissionUpdateRequest request, bool codeAlreadyExists, bool permissionExists)
   {
      string code = Permission.BuildCode(request.Module, request.Resource, request.Action);

      var validator = new FluentValidator<PermissionUpdateRequest>()
         .RuleFor(x => x.Module).ApplyTemplate(MemberCodeTemplate)
         .RuleFor(x => x.Resource).ApplyTemplate(MemberCodeTemplate)
         .RuleFor(x => x.Action).ApplyTemplate(MemberCodeTemplate)
         .RuleForValue(codeAlreadyExists).IsFalse(new PermissionDuplicateError(code))
         .RuleForValue(permissionExists).IsTrue(new NotFoundError(IamConst.Entity.Permission));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateAssign(RolePermissionAssignRequest request, bool roleExists, bool allPermissionsExist)
   {
      var validator = new FluentValidator<RolePermissionAssignRequest>()
         .RuleForValue(roleExists).IsTrue(new NotFoundError(IamConst.Entity.Role))
         .RuleForValue(allPermissionsExist).IsTrue(new PermissionNotFoundInAssignmentError())
         .RuleFor(x => x.PermissionIds).HasItems().HasNoDuplicates();

      var isValid = validator.Validate(request, shortCircuitMode: true);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUnassign(RolePermissionUnassignRequest request, bool roleExists, bool roleHasAllPermissions)
   {
      var validator = new FluentValidator<RolePermissionUnassignRequest>()
         .RuleForValue(roleExists).IsTrue(new NotFoundError(IamConst.Entity.Role))
         .RuleForValue(roleHasAllPermissions).IsTrue(new PermissionsCannotBeUnassignedError())
         .RuleFor(x => x.PermissionIds).HasItems().HasNoDuplicates();

      var isValid = validator.Validate(request, shortCircuitMode: true);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
