using Shared.Domain.Enums;

namespace Courier.Application.Contracts;

public interface ICourierLogger
{
   Task LogAuditAsync(
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      RetentionPolicy retentionPolicy,
      string description,
      Guid organizationId,
      Guid userId,
      Guid? targetId = null,
      object? metadata = null,
      CancellationToken cancellationToken = default);

   Task LogSystemAsync(
      SystemLogLevel level,
      SystemLogStatus status,
      string message,
      Exception? exception = null,
      Guid? organizationId = null,
      Guid? userId = null,
      Dictionary<string, object>? properties = null,
      CancellationToken cancellationToken = default);
}
