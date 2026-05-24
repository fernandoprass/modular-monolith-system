using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Messages;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Courier.Application.Services;

public class EmailOutboxService(
   IEmailRepository emailRepository,
   IEmailTemplateRepository emailTemplateRepository,
   IEmailTemplateRenderer emailTemplateRenderer,
   IEmailSender emailSender,
   IParameterService parameterService,
   ICourierLogger courierLogger) : IEmailOutboxService
{
   private readonly IEmailRepository _emailRepository = emailRepository;
   private readonly IEmailTemplateRepository _emailTemplateRepository = emailTemplateRepository;
   private readonly IEmailTemplateRenderer _emailTemplateRenderer = emailTemplateRenderer;
   private readonly IEmailSender _emailSender = emailSender;
   private readonly IParameterService _parameterService = parameterService;
   private readonly ICourierLogger _courierLogger = courierLogger;

   public async Task<Result<Guid>> QueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default)
   {
      var template = await _emailTemplateRepository.GetByKeyAsync(request.TemplateKey, cancellationToken);

      if (template == null)
      {
         return Result<Guid>.Failure(new NotFoundError(CourierConst.Entity.EmailTemplate));
      }

      var translation = template.Translations.SingleOrDefault(t =>
         t.Language == request.Language.ToLowerInvariant().Trim());

      if (translation == null)
      {
         return Result<Guid>.Failure(new EmailTemplateLanguageNotFoundError(request.TemplateKey, request.Language));
      }

      var values = BuildTemplateValues(request.Values);
      var subjectResult = _emailTemplateRenderer.Render(translation.Subject, values);

      if (subjectResult.HasError)
      {
         return Result<Guid>.Failure(subjectResult.Messages);
      }

      var bodyResult = _emailTemplateRenderer.Render(translation.Body, values, translation.IsHtml);

      if (bodyResult.HasError)
      {
         return Result<Guid>.Failure(bodyResult.Messages);
      }

      var email = Email.Create(
         request.OrganizationId,
         request.UserId,
         request.Module,
         request.Feature,
         request.TemplateKey,
         request.Recipient,
         subjectResult.Data!,
         bodyResult.Data!,
         translation.IsHtml,
         template.RetentionPolicy);

      var id = await _emailRepository.AddAsync(email, cancellationToken);

      await _courierLogger.LogAuditAsync(
         CourierConst.Logger.Feature.Emails,
         CourierConst.Logger.Action.Queue,
         AuditPrivacyLevel.Medium,
         $"Queued email {id}",
         email.OrganizationId,
         email.UserId,
         id,
         new { email.TemplateKey, email.Recipient, email.Subject },
         cancellationToken);

      return Result<Guid>.Success(id);
   }

   public async Task<bool> ProcessNextPendingAsync(CancellationToken cancellationToken = default)
   {
      var email = await _emailRepository.ClaimNextPendingAsync(DateTime.UtcNow, cancellationToken);

      if (email == null)
      {
         return false;
      }

      await SendClaimedEmailAsync(email, cancellationToken);
      return true;
   }

   private async Task SendClaimedEmailAsync(Email email, CancellationToken cancellationToken)
   {
      try
      {
         var sendResult = await _emailSender.SendAsync(email, cancellationToken);

         if (sendResult.HasError)
         {
            await RecordFailureAsync(email, string.Join(" | ", sendResult.Messages.Select(m => m.Show())), null, cancellationToken);
            return;
         }

         await RecordSentAsync(email, cancellationToken);
      }
      catch (Exception ex)
      {
         await RecordFailureAsync(email, ex.Message, ex.StackTrace, cancellationToken);
      }
   }

   private async Task RecordSentAsync(Email email, CancellationToken cancellationToken)
   {
      email.MarkAsSent();
      await _emailRepository.UpdateAsync(email, cancellationToken);

      await _courierLogger.LogAuditAsync(
         CourierConst.Logger.Feature.Emails,
         CourierConst.Logger.Action.Send,
         AuditPrivacyLevel.Medium,
         $"Sent email {email.Id}",
         email.OrganizationId,
         email.UserId,
         email.Id,
         new { email.TemplateKey, email.Recipient, email.Subject },
         cancellationToken);
   }

   private async Task RecordFailureAsync(Email email, string message, string? stackTrace, CancellationToken cancellationToken)
   {
      var maxRetries = await GetMaxRetriesAsync(email, cancellationToken);

      email.RecordFailure(message, stackTrace, maxRetries);
      await _emailRepository.UpdateAsync(email, cancellationToken);

      await _courierLogger.LogAuditAsync(
         CourierConst.Logger.Feature.Emails,
         CourierConst.Logger.Action.Fail,
         AuditPrivacyLevel.High,
         $"Failed email {email.Id}",
         email.OrganizationId,
         email.UserId,
         email.Id,
         new { email.TemplateKey, email.Recipient, email.Subject, email.RetryCount, email.Status },
         cancellationToken);

      await _courierLogger.LogSystemAsync(
         SystemLogLevel.Error,
         SystemLogStatus.Failure,
         $"Failed to send email {email.Id}",
         null,
         email.OrganizationId,
         email.UserId,
         new Dictionary<string, object>
         {
            ["emailId"] = email.Id,
            ["templateKey"] = email.TemplateKey,
            ["recipient"] = email.Recipient,
            ["retryCount"] = email.RetryCount,
            ["error"] = message
         },
         cancellationToken);
   }

   private async Task<int> GetMaxRetriesAsync(Email email, CancellationToken cancellationToken)
   {
      try
      {
         var maxRetries = await _parameterService.GetIntAsync(CourierParam.EmailDelivery.MaxRetries, cancellationToken);
         return maxRetries > 0 ? maxRetries : CourierConst.Worker.DefaultMaxRetries;
      }
      catch (Exception ex)
      {
         await _courierLogger.LogSystemAsync(
            SystemLogLevel.Warning,
            SystemLogStatus.Failure,
            "Failed to load Courier max retries parameter. Using default.",
            ex,
            email.OrganizationId,
            email.UserId,
            new Dictionary<string, object>
            {
               ["emailId"] = email.Id,
               ["parameter"] = CourierParam.EmailDelivery.MaxRetries,
               ["defaultMaxRetries"] = CourierConst.Worker.DefaultMaxRetries
            },
            cancellationToken);

         return CourierConst.Worker.DefaultMaxRetries;
      }
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
