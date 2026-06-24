using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Messages;
using Myce.FluentValidator;
using Myce.Response;
using Shared.Domain;
using Shared.Domain.Messages;

namespace Courier.Application.Validators;

public class TemplateValidator : ITemplateValidator
{
   private static void KeyTemplate<T>(RuleBuilder<T, string> rb) where T : class
      => rb.IsRequired().MinLength(5);

   private static void LanguageTemplate<T>(RuleBuilder<T, string> rb) where T : class
      => rb.IsRequired().MinLength(2).MaxLength(35);

   public Result ValidateCreate(TemplateCreateRequest request, bool keyExists)
   {
      var validator = new FluentValidator<TemplateCreateRequest>()
         .RuleFor(x => x.Module).IsRequired().MinLength(2)
         .RuleFor(x => x.Key).ApplyTemplate(KeyTemplate)
         .RuleFor(x => x.Severity).IsRequired().IsInEnum()
         .RuleFor(x => x.RetentionPolicy).IsRequired().IsInEnum()
         .RuleForValue(keyExists).IsFalse(new TemplateDuplicateKeyError(request.Module, request.Key));

      var isValid = validator.Validate(request);
      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }

   public Result ValidateUpdate(TemplateUpdateRequest request, bool templateExists, bool keyExists)
   {
      var validator = new FluentValidator<TemplateUpdateRequest>()
         .RuleFor(x => x.Module).IsRequired().MinLength(2)
         .RuleFor(x => x.Key).ApplyTemplate(KeyTemplate)
         .RuleFor(x => x.Severity).IsRequired().IsInEnum()
         .RuleFor(x => x.RetentionPolicy).IsRequired().IsInEnum()
         .RuleForValue(templateExists).IsTrue(new NotFoundError(CourierConst.Entity.Template))
         .RuleForValue(keyExists).IsFalse(new TemplateDuplicateKeyError(request.Module, request.Key));

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

   public Result ValidateTranslation(TemplateTranslationRequest request, bool templateExists)
   {
      var validator = new FluentValidator<TemplateTranslationRequest>()
         .RuleFor(x => x.Language).ApplyTemplate(LanguageTemplate)
         .RuleFor(x => x.Name).IsRequired()
         .RuleForValue(LanguageOptions.IsSupported(request.Language)).IsTrue(new InvalidLanguageError(request.Language))
         .RuleForValue(templateExists).IsTrue(new NotFoundError(CourierConst.Entity.Template))
         .RuleForValue(request.Email != null || request.Notification != null).IsTrue(new TemplateChannelRequiredError());

      if (!validator.Validate(request))
      {
         return Result.Failure(validator.Messages);
      }

      if (request.Email != null)
      {
         var emailValidator = new FluentValidator<TemplateTranslationEmailRequest>()
            .RuleFor(x => x.Subject).IsRequired().MinLength(10)
            .RuleFor(x => x.Body).IsRequired();

         if (!emailValidator.Validate(request.Email))
         {
            return Result.Failure(emailValidator.Messages);
         }
      }

      if (request.Notification != null)
      {
         var notificationValidator = new FluentValidator<TemplateTranslationNotificationRequest>()
            .RuleFor(x => x.Title).IsRequired()
            .RuleFor(x => x.Message).IsRequired();

         if (!notificationValidator.Validate(request.Notification))
         {
            return Result.Failure(notificationValidator.Messages);
         }
      }

      return Result.Success();
   }
}
