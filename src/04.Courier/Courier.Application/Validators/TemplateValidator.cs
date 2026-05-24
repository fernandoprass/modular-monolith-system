using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Enums;
using Courier.Domain.Messages;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Domain.Messages;

namespace Courier.Application.Validators;

public class TemplateValidator : ITemplateValidator
{
   private static void KeyTemplate<T>(RuleBuilder<T, string> rb) where T : class
                  => rb.IsRequired().MinLength(5);

   private static void LanguageTemplate<T>(RuleBuilder<T, string> rb) where T : class
               => rb.IsRequired().MinLength(2).MaxLength(5);

   public Result ValidateCreate(TemplateCreateRequest request, bool keyExists)
   {
      var validator = new FluentValidator<TemplateCreateRequest>()
         .RuleFor(x => x.Key).ApplyTemplate(KeyTemplate)
         .RuleFor(x => x.Type).IsRequired().IsInEnum()
         .RuleFor(x => x.RetentionPolicy).IsRequired().IsInEnum()
         .RuleForValue(keyExists).IsFalse(new TemplateDuplicateKeyError(request.Key));

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(TemplateUpdateRequest request, bool templateExists, bool keyExists)
   {
      var validator = new FluentValidator<TemplateUpdateRequest>()
         .RuleFor(x => x.Key).ApplyTemplate(KeyTemplate)
         .RuleFor(x => x.Type).IsRequired().IsInEnum()
         .RuleFor(x => x.RetentionPolicy).IsRequired().IsInEnum()
         .RuleForValue(templateExists).IsTrue(new NotFoundError(CourierConst.Entity.Template))
         .RuleForValue(keyExists).IsFalse(new TemplateDuplicateKeyError(request.Key));

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateSearch(TemplateSearchRequest request)
   {
      var validator = new FluentValidator<TemplateSearchRequest>()
         .RuleFor(x => x.PageNumber).IsGreaterThan(0)
         .RuleFor(x => x.PageSize).IsGreaterThan(0);

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateEmailTranslation(TemplateEmailTranslationRequest request, bool templateExists, bool isEmailTemplate)
   {
      var validator = new FluentValidator<TemplateEmailTranslationRequest>()
         .RuleFor(x => x.Language).ApplyTemplate(LanguageTemplate)
         .RuleFor(x => x.Subject).IsRequired().MinLength(10)
         .RuleFor(x => x.Body).IsRequired()
         .RuleForValue(templateExists).IsTrue(new NotFoundError(CourierConst.Entity.Template))
         .RuleForValue(isEmailTemplate).IsTrue(new TemplateTypeMismatchError(TemplateType.Email.ToString()));

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
