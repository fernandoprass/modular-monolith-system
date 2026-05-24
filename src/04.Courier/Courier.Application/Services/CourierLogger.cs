using Courier.Application.Contracts;
using Courier.Domain;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using System.Text.Json;

namespace Courier.Application.Services;

public class CourierLogger(IEventPublisher eventPublisher) : ICourierLogger
{
   private readonly IEventPublisher _eventPublisher = eventPublisher;

   public async Task LogAuditAsync(
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      string description,
      Guid organizationId,
      Guid userId,
      Guid? targetId = null,
      object? metadata = null,
      CancellationToken cancellationToken = default)
   {
      var auditLog = new AuditLogEvent
      {
         Module = CourierConst.System.ModuleName.ToLowerInvariant(),
         Feature = feature,
         Action = action,
         PrivacyLevel = privacyLevel,
         Description = description,
         OrganizationId = organizationId,
         UserId = userId,
         TargetId = targetId ?? Guid.Empty,
         Metadata = JsonSerializer.Serialize(metadata ?? new { })
      };

      await _eventPublisher.PublishAuditLogEventAsync(auditLog, cancellationToken);
   }

   public async Task LogSystemAsync(
      SystemLogLevel level,
      SystemLogStatus status,
      string message,
      Exception? exception = null,
      Guid? organizationId = null,
      Guid? userId = null,
      Dictionary<string, object>? properties = null,
      CancellationToken cancellationToken = default)
   {
      var systemLog = new SystemLogEvent
      {
         Source = CourierConst.System.ModuleName,
         Level = level,
         Status = status,
         Message = message,
         Exception = exception?.Message,
         StackTrace = exception?.StackTrace,
         OrganizationId = organizationId,
         UserId = userId,
         Properties = properties ?? []
      };

      await _eventPublisher.PublishSystemLogEventAsync(systemLog, cancellationToken);
   }
}
