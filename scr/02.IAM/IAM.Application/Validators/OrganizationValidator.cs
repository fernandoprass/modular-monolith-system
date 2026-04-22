using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Enums;
using IAM.Domain.Messages;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Domain.Messages;

namespace IAM.Application.Validators;

public class OrganizationValidator : IOrganizationValidator
{
   private static void CodeRules<T>(RuleBuilder<T, string> rb) where T : class
                           => rb.IsRequired().MinLength(3).IsAlphaNumeric();

   public Result ValidateCreate(OrganizationCreateRequest request, bool newCodeExists)
   {
      var validator = new FluentValidator<OrganizationCreateRequest>()
          .RuleFor(x => x.Type).IsInEnum(new OrganizationInvalidTypeError())
          .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplates.NameRules)
          .RuleFor(x => x.Code).If(x => x.Type.Equals(OrganizationType.Company), x => x.ApplyTemplate(CodeRules))
          .RuleFor(x => x.User).IsNotNull() //just to ensure the user object is provided, validation is done in UserValidator
          .RuleForValue(newCodeExists).IsFalse(new OrganizationDuplicateCodeError(request.Code));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(OrganizationUpdateRequest request, bool organizationExists)
   {
      var validator = new FluentValidator<OrganizationUpdateRequest>()
          .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplates.NameRules)
          .RuleForValue(organizationExists).IsTrue(new NotFoundError(IamConst.Entity.Organization));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdateCode(OrganizationUpdateCodeRequest request, bool newCodeExists)
   {
      var validator = new FluentValidator<OrganizationUpdateCodeRequest>()
          .RuleFor(x => x.Code).ApplyTemplate(CodeRules)
          .RuleForValue(newCodeExists).IsFalse(new OrganizationDuplicateCodeError(request.Code));

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
