using Shared.Domain.Enums;

namespace Shared.Domain.Events;

public record SystemLogEvent
{
   public Guid Id { get; init; } = Guid.NewGuid();
   public DateTime Timestamp { get; init; } = DateTime.UtcNow;
   public SystemLogLevel Level { get; init; } = SystemLogLevel.Information;
   public SystemLogStatus Status { get; init; } = SystemLogStatus.Unknown;
   public string Source { get; init; } = string.Empty;
   public string Message { get; init; } = string.Empty;
   public string? Exception { get; init; }
   public string? StackTrace { get; init; }
   public string? RequestId { get; init; }
   public Guid? OrganizationId { get; init; }
   public Guid? UserId { get; init; }
   public Dictionary<string, object> Properties { get; init; } = [];
}
