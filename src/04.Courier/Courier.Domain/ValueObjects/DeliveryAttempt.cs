namespace Courier.Domain.ValueObjects;

public record DeliveryAttempt(
   DateTime AttemptedAt,
   string ErrorMessage,
   string? StackTrace);
