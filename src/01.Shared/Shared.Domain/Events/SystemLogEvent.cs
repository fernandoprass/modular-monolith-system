using Shared.Domain.Enums;

namespace Shared.Domain.Events;

public record SystemLogEvent
{
   public Guid Id { get; init; } = Guid.CreateVersion7();
   public SystemLogLevel Level { get; init; } = SystemLogLevel.Information;
   public SystemLogStatus Status { get; init; } = SystemLogStatus.Unknown;
   public RetentionPolicy RetentionPolicy { get; init; } = RetentionPolicy.Standard;
   public string Module { get; init; } = string.Empty;
   public string Message { get; init; } = string.Empty;
   public string? Exception { get; init; }
   public string? StackTrace { get; init; }
   public string? RequestId { get; init; }
   public Guid? OrganizationId { get; init; }
   public Guid? UserId { get; init; }
   public Dictionary<string, object> Properties { get; init; } = [];

   public SystemLogEvent() { }
}
