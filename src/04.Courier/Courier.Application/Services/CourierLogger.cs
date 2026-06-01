using Courier.Application.Contracts;
using Courier.Domain;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Courier.Application.Services;

public class CourierLogger(IEventPublisher eventPublisher) : ICourierLogger
{
   private readonly IEventPublisher _eventPublisher = eventPublisher;

   public async Task LogAuditAsync(
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      RetentionPolicy retentionPolicy,
      string description,
      Guid organizationId,
      Guid userId,
      Guid? targetId = null,
      object? metadata = null,
      CancellationToken cancellationToken = default)
   {

      var auditLog = AuditLogEvent.Create(
         module: CourierConst.System.ModuleName.ToLowerInvariant(),
         feature: feature,
         action: action,
         description: description,
         privacyLevel: privacyLevel,
         retentionPolicy: retentionPolicy,
         ipAddress: null,
         userAgent: null,
         userId: userId,
         targetId: targetId ?? Guid.Empty,
         organizationId: organizationId,
         metadata: JsonSerializer.Serialize(metadata ?? new { })
      );

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
