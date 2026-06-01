using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Courier.Domain.Entities;

public class Email : Entity
{
   public Guid OrganizationId { get; private set; }
   public Guid UserId { get; private set; }
   public string Module { get; private set; } = string.Empty;
   public string Feature { get; private set; } = string.Empty;
   public string TemplateKey { get; private set; } = string.Empty;
   public string Recipient { get; private set; } = string.Empty;
   public string Subject { get; private set; } = string.Empty;
   public string Body { get; private set; } = string.Empty;
   public bool IsHtml { get; private set; }
   public DateTime CreatedAt { get; private set; }
   public DateTime? SentAt { get; private set; }
   public DateTime ExpiresAt { get; private set; }
   public EmailStatus Status { get; private set; }
   public int RetryCount { get; private set; }
   public DateTime? NextAttemptAt { get; private set; }

   private readonly List<DeliveryAttempt> _attempts = new();
   public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

   private Email() { }

   public static Email Create(
      Guid organizationId,
      Guid userId,
      string module,
      string feature,
      string templateKey,
      string recipient,
      string subject,
      string body,
      bool isHtml,
      RetentionPolicy retentionPolicy)
   {
      var now = DateTime.UtcNow;

      return new Email
      {
         Id = Guid.CreateVersion7(),
         OrganizationId = organizationId,
         UserId = userId,
         Module = module.Trim(),
         Feature = feature.Trim(),
         TemplateKey = templateKey.Trim(),
         Recipient = recipient.ToLowerInvariant().Trim(),
         Subject = subject.Trim(),
         Body = body,
         IsHtml = isHtml,
         CreatedAt = now,
         ExpiresAt = now.AddDays(GetRetentionDays(retentionPolicy)),
         Status = EmailStatus.Pending,
         RetryCount = 0,
         NextAttemptAt = now,
         SentAt = null
      };
   }

   private static int GetRetentionDays(RetentionPolicy retentionPolicy)
   {
      return retentionPolicy switch
      {
         RetentionPolicy.Operational => CourierConst.EmailRetentionPoliciesTimeSpans.Operational,
         RetentionPolicy.Standard => CourierConst.EmailRetentionPoliciesTimeSpans.Standard,
         RetentionPolicy.Extended => CourierConst.EmailRetentionPoliciesTimeSpans.Extended,
         RetentionPolicy.Compliance => CourierConst.EmailRetentionPoliciesTimeSpans.Compliance,
         RetentionPolicy.LongTerm => CourierConst.EmailRetentionPoliciesTimeSpans.LongTerm,
         _ => CourierConst.EmailRetentionPoliciesTimeSpans.Standard
      };
   }

   public void MarkAsProcessing()
   {
      if (Status == EmailStatus.Sent)
      {
         return;
      }

      Status = EmailStatus.Processing;
   }

   public void MarkAsSent()
   {
      Status = EmailStatus.Sent;
      SentAt = DateTime.UtcNow;
      NextAttemptAt = null;
   }

   public void RecordFailure(string errorMessage, string? stackTrace, int maxRetriesThreshold)
   {
      _attempts.Add(new DeliveryAttempt(DateTime.UtcNow, errorMessage, stackTrace));
      RetryCount++;

      if (RetryCount >= maxRetriesThreshold)
      {
         Status = EmailStatus.Failed;
         NextAttemptAt = null;
      }
      else
      {
         Status = EmailStatus.Pending;

         var minutesToWait = Math.Pow(2, RetryCount);
         NextAttemptAt = DateTime.UtcNow.AddMinutes(minutesToWait);
      }
   }
}
