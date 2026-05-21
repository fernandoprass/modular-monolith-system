using Courier.Application.Contracts;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Messages;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Domain.Messages;

namespace Courier.Application.Validators;

public class EmailTemplateValidator : IEmailTemplateValidator
{
   public Result ValidateCreate(EmailTemplateCreateRequest request, bool keyExists)
   {
      var validator = new FluentValidator<EmailTemplateCreateRequest>()
         .RuleFor(x => x.Key).IsRequired().MinLength(2)
         .RuleFor(x => x.RetentionPolicy).IsRequired().IsInEnum()
         .RuleForValue(keyExists).IsFalse(new EmailTemplateDuplicateKeyError(request.Key));

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(EmailTemplateUpdateRequest request, bool templateExists, bool keyExists)
   {
      var validator = new FluentValidator<EmailTemplateUpdateRequest>()
         .RuleFor(x => x.Key).IsRequired().MinLength(2)
         .RuleFor(x => x.RetentionPolicy).IsRequired().IsInEnum()
         .RuleForValue(templateExists).IsTrue(new NotFoundError(Courier.Domain.CourierConst.Entity.EmailTemplate))
         .RuleForValue(keyExists).IsFalse(new EmailTemplateDuplicateKeyError(request.Key));

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateSearch(EmailTemplateSearchRequest request)
   {
      var validator = new FluentValidator<EmailTemplateSearchRequest>()
         .RuleFor(x => x.PageNumber).Custom(x => x > 0, new EmailInvalidPageNumberError())
         .RuleFor(x => x.PageSize).Custom(x => x > 0, new EmailInvalidPageSizeError());

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateTranslation(EmailTemplateTranslationRequest request)
   {
      var validator = new FluentValidator<EmailTemplateTranslationRequest>()
         .RuleFor(x => x.Language).IsRequired().MinLength(2)
         .RuleFor(x => x.Name).IsRequired().MinLength(2)
         .RuleFor(x => x.Subject).IsRequired().MinLength(2)
         .RuleFor(x => x.Body).IsRequired();

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
