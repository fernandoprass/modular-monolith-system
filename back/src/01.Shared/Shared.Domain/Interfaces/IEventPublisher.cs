using Shared.Domain.Events;

namespace Shared.Domain.Interfaces;

public interface IEventPublisher
{
   Task PublishAuditLogEventAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default);
   Task PublishSystemLogEventAsync(SystemLogEvent systemLog, CancellationToken cancellationToken = default);
   Task PublishUserMessageEventAsync(UserMessageEvent messageRequest, CancellationToken cancellationToken = default);
}
