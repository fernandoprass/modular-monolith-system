using Courier.Application.Contracts;
using Courier.Domain.DTOs.Requests;
using Myce.FluentValidator;
using Myce.Response;

namespace Courier.Application.Validators;

public class EmailValidator : IEmailValidator
{
   public Result ValidateCreate(EmailCreateRequest request)
   {
      var validator = new FluentValidator<EmailCreateRequest>()
         .RuleFor(x => x.OrganizationId).IsRequired()
         .RuleFor(x => x.UserId).IsRequired()
         .RuleFor(x => x.Module).IsRequired().MinLength(2)
         .RuleFor(x => x.Feature).IsRequired().MinLength(2)
         .RuleFor(x => x.TemplateKey).IsRequired().MinLength(2)
         .RuleFor(x => x.Recipient).IsRequired().IsValidEmailAddress()
         .RuleFor(x => x.Subject).IsRequired().MinLength(2)
         .RuleFor(x => x.Body).IsRequired();

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateSearch(EmailSearchRequest request)
   {
      var validator = new FluentValidator<EmailSearchRequest>()
         .RuleFor(x => x.DateFrom).IsRequired()
         .RuleFor(x => x.DateTo).IsRequired().IsGreaterThanOrEqualTo(x => x.DateFrom)
         .RuleFor(x => x.PageNumber).IsGreaterThan(0)
         .RuleFor(x => x.PageSize).IsGreaterThan(0);

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
