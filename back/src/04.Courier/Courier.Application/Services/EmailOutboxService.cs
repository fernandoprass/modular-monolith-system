using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Courier.Application.Services;

public class EmailOutboxService(
   IEmailRepository emailRepository,
   IEmailSender emailSender,
   IParameterService parameterService,
   ICourierLogger courierLogger) : IEmailOutboxService
{
   private readonly IEmailRepository _emailRepository = emailRepository;
   private readonly IEmailSender _emailSender = emailSender;
   private readonly IParameterService _parameterService = parameterService;
   private readonly ICourierLogger _courierLogger = courierLogger;

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
   }

   private async Task RecordFailureAsync(Email email, string message, string? stackTrace, CancellationToken cancellationToken)
   {
      var maxRetries = await GetMaxRetriesAsync(email, cancellationToken);

      email.RecordFailure(message, stackTrace, maxRetries);
      await _emailRepository.UpdateAsync(email, cancellationToken);

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

}
