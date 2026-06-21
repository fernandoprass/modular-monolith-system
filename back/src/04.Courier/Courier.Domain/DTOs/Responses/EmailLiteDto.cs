using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record EmailLiteDto(
   Guid Id,
   string Module,
   string Feature,
   string Recipient,
   string Subject,
   EmailStatus Status);
