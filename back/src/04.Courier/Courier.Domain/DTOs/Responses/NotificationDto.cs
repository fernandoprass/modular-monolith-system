using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record NotificationDto(
   Guid Id,
   Guid OrganizationId,
   Guid UserId,
   string Module,
   string Feature,
   string TemplateKey,
   string Title,
   string Message,
   string ActionLink,
   NotificationStatus Status,
   DateTime CreatedAt,
   DateTime? ReadAt,
   DateTime ExpiresAt);
