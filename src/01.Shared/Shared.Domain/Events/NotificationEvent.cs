namespace Shared.Domain.Events;

public record NotificationEvent
{
   public Guid Id { get; init; } = Guid.CreateVersion7();
   public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
   public string Type { get; init; } = string.Empty;
   public Guid UserId { get; init; }
   public string Title { get; init; } = string.Empty;
   public string Message { get; init; } = string.Empty;
   public string? ActionUrl { get; init; }
   public string Severity { get; init; } = "info";
   public Dictionary<string, object> Data { get; init; } = [];
}
