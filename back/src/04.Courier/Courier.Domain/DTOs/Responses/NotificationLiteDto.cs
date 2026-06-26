using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record NotificationLiteDto(
   Guid Id,
   string Module,
   string Feature,
   string Title,
   string Message,
   string ActionLink,
   NotificationStatus Status,
   DateTime CreatedAt,
   DateTime? ReadAt);
