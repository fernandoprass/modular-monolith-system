using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record EmailLiteDto(
   Guid Id,
   Guid OrganizationId,
   Guid UserId,
   string Module,
   string Feature,
   string TemplateKey,
   string Recipient,
   string Subject,
   bool IsHtml,
   DateTime CreatedAt,
   DateTime? SentAt,
   DateTime ExpiresAt,
   EmailStatus Status,
   int RetryCount,
   DateTime? NextAttemptAt);
