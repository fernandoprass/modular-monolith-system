namespace Shared.Domain.Events;

public record IntegrationEvent<TPayload>(
   Guid EventId,
   string EventName,
   int Version,
   Guid? CorrelationId,
   DateTime CreatedAt,
   TPayload Payload)
{
   public static IntegrationEvent<TPayload> Create(
      string eventName,
      int version,
      TPayload payload,
      Guid? correlationId = null)
   {
      return new IntegrationEvent<TPayload>(
         EventId: Guid.CreateVersion7(),
         eventName,
         version,
         correlationId,
         CreatedAt: DateTime.UtcNow,
         payload);
   }
}
