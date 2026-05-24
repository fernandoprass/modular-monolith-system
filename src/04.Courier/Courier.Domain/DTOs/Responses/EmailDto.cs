using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;

namespace Courier.Domain.DTOs.Responses;

public record EmailDto(
   Guid Id,
   Guid OrganizationId,
   Guid UserId,
   string Module,
   string Feature,
   string TemplateKey,
   string Recipient,
   string Subject,
   string Body,
   bool IsHtml,
   DateTime CreatedAt,
   DateTime? SentAt,
   DateTime ExpiresAt,
   EmailStatus Status,
   int RetryCount,
   DateTime? NextAttemptAt,
   IReadOnlyCollection<DeliveryAttempt> Attempts);
