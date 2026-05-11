using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class SystemLog : Entity
{
   public DateTime Timestamp { get; private set; }
   public SystemLogLevel Level { get; private set; }
   public SystemLogStatus Status { get; private set; } = SystemLogStatus.Unknown;
   public string Source { get; private set; } = string.Empty;
   public string Message { get; private set; } = string.Empty;
   public string? Exception { get; private set; }
   public string? StackTrace { get; private set; }
   public string? RequestId { get; private set; }
   public Guid? UserId { get; private set; }
   public Guid? OrganizationId { get; private set; }
   public string PropertiesJson { get; private set; } = "{}";

   private SystemLog() { }

   public static SystemLog Create(
      DateTime timestamp,
      SystemLogLevel level,
      SystemLogStatus status,
      string source,
      string message,
      string? exception,
      string? stackTrace,
      string? requestId,
      Guid? userId,
      Guid? organizationId,
      string propertiesJson)
   {
      return new SystemLog
      {
         Id = Guid.CreateVersion7(),
         Timestamp = timestamp,
         Level = level,
         Status = status,
         Source = source,
         Message = message,
         Exception = exception,
         StackTrace = stackTrace,
         RequestId = requestId,
         UserId = userId,
         OrganizationId = organizationId,
         PropertiesJson = string.IsNullOrWhiteSpace(propertiesJson) ? "{}" : propertiesJson
      };
   }
}
