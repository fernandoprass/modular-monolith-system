using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Messages;
using Courier.Domain.ValueObjects;
using Myce.Response;
using Shared.Domain;
using Shared.Domain.Messages;

namespace Courier.Application.Services;

public class CourierMessageService(
   IEmailRepository emailRepository,
   INotificationRepository notificationRepository,
   ITemplateRepository templateRepository,
   IEmailTemplateRenderer templateRenderer,
   IEmailValidator emailValidator) : ICourierMessageService
{
   private readonly IEmailRepository _emailRepository = emailRepository;
   private readonly INotificationRepository _notificationRepository = notificationRepository;
   private readonly ITemplateRepository _templateRepository = templateRepository;
   private readonly IEmailTemplateRenderer _templateRenderer = templateRenderer;
   private readonly IEmailValidator _emailValidator = emailValidator;

   public async Task<Result> QueueAsync(CourierMessageRequest request, CancellationToken cancellationToken = default)
   {
      var template = await _templateRepository.GetByModuleAndKeyAsync(
         request.Module,
         request.TemplateKey,
         cancellationToken);

      if (template == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.Template));
      }

      var translation = FindTranslation(template, request.Language);

      if (translation == null)
      {
         return Result.Failure(new TemplateLanguageNotFoundError(request.TemplateKey, request.Language));
      }

      if (translation.Email == null && translation.Notification == null)
      {
         return Result.Failure(new TemplateChannelRequiredError());
      }

      var values = BuildTemplateValues(request.Values);

      if (translation.Email != null)
      {
         var emailResult = await QueueEmailAsync(request, template, translation.Email, values, cancellationToken);

         if (emailResult.HasError)
         {
            return emailResult;
         }
      }

      if (translation.Notification != null)
      {
         var notificationResult = await QueueNotificationAsync(request, template, translation.Notification, values, cancellationToken);

         if (notificationResult.HasError)
         {
            return notificationResult;
         }
      }

      return Result.Success();
   }

   private async Task<Result> QueueEmailAsync(
      CourierMessageRequest request,
      Template template,
      TemplateTranslationEmail emailTemplate,
      IReadOnlyDictionary<string, string> values,
      CancellationToken cancellationToken)
   {
      var subjectResult = _templateRenderer.Render(emailTemplate.Subject, values);

      if (subjectResult.HasError)
      {
         return Result.Failure(subjectResult.Messages);
      }

      var bodyResult = _templateRenderer.Render(emailTemplate.Body, values, emailTemplate.IsHtml);

      if (bodyResult.HasError)
      {
         return Result.Failure(bodyResult.Messages);
      }

      var emailCreateRequest = new EmailCreateRequest(
         request.OrganizationId,
         request.UserId,
         request.Module,
         request.Feature,
         request.TemplateKey,
         request.Recipient ?? string.Empty,
         subjectResult.Data!,
         bodyResult.Data!,
         emailTemplate.IsHtml);
      var validation = _emailValidator.ValidateCreate(emailCreateRequest);

      if (validation.HasError)
      {
         return validation;
      }

      var email = Email.Create(
         emailCreateRequest.OrganizationId,
         emailCreateRequest.UserId,
         emailCreateRequest.Module,
         emailCreateRequest.Feature,
         emailCreateRequest.TemplateKey,
         emailCreateRequest.Recipient,
         emailCreateRequest.Subject,
         emailCreateRequest.Body,
         emailCreateRequest.IsHtml,
         template.RetentionPolicy);

      await _emailRepository.AddAsync(email, cancellationToken);
      return Result.Success();
   }

   private async Task<Result> QueueNotificationAsync(
      CourierMessageRequest request,
      Template template,
      TemplateTranslationNotification notificationTemplate,
      IReadOnlyDictionary<string, string> values,
      CancellationToken cancellationToken)
   {
      var titleResult = _templateRenderer.Render(notificationTemplate.Title, values);

      if (titleResult.HasError)
      {
         return Result.Failure(titleResult.Messages);
      }

      var messageResult = _templateRenderer.Render(notificationTemplate.Message, values);

      if (messageResult.HasError)
      {
         return Result.Failure(messageResult.Messages);
      }

      var actionLinkResult = notificationTemplate.ActionLink == null
         ? Result<string>.Success(string.Empty)
         : _templateRenderer.Render(notificationTemplate.ActionLink, values);

      if (actionLinkResult.HasError)
      {
         return Result.Failure(actionLinkResult.Messages);
      }

      var notification = Notification.Create(
         request.OrganizationId,
         request.UserId,
         request.Module,
         request.Feature,
         request.TemplateKey,
         titleResult.Data!,
         messageResult.Data!,
         actionLinkResult.Data!,
         template.RetentionPolicy);

      await _notificationRepository.AddAsync(notification, cancellationToken);
      return Result.Success();
   }

   private static TemplateTranslation? FindTranslation(Template template, string language)
   {
      var normalizedLanguage = LanguageOptions.Normalize(language);
      var defaultLanguage = LanguageOptions.Normalize(SharedConst.System.DefaultLanguage);

      return template.Translations.SingleOrDefault(t => t.Language == normalizedLanguage)
         ?? template.Translations.SingleOrDefault(t => t.Language == defaultLanguage);
   }

   private static IReadOnlyDictionary<string, string> BuildTemplateValues(IReadOnlyDictionary<string, string>? values)
   {
      var result = values == null
         ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
         : new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);

      result["today"] = DateTime.UtcNow.ToString("yyyy-MM-dd");

      return result;
   }
}
