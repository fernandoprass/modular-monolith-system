using Shared.Domain.Events;

namespace Shared.Domain.Interfaces;

public interface IEventPublisher
{
   Task PublishAuditLogEventAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default);
   Task PublishSystemLogEventAsync(SystemLogEvent systemLog, CancellationToken cancellationToken = default);
   Task PublishEmailRequestedEventAsync(EmailRequestedEvent emailRequest, CancellationToken cancellationToken = default);
   Task PublishNotificationEventAsync(NotificationEvent notification, CancellationToken cancellationToken = default);
}
