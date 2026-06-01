using Shared.Domain.Enums;

namespace Shared.Domain.Events;

public record NotificationEvent
{
   public Guid Id { get; init; } = Guid.CreateVersion7();
   public string Type { get; init; } = string.Empty;
   public string Title { get; init; } = string.Empty;
   public string Message { get; init; } = string.Empty;
   public string? ActionUrl { get; init; }
   public string Severity { get; init; } = "info";
   public RetentionPolicy RetentionPolicy { get; init; } = RetentionPolicy.Standard;
   public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
   public Guid UserId { get; init; }
   public Dictionary<string, object> Data { get; init; } = [];
}
