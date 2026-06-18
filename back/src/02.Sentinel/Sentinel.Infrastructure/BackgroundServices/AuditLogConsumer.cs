using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentinel.Domain;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Shared.Domain;
using Shared.Domain.Events;
using StackExchange.Redis;

namespace Sentinel.Infrastructure.BackgroundServices;

public class AuditLogConsumer(
   IConnectionMultiplexer redis,
   IServiceProvider serviceProvider,
   ILogger<AuditLogConsumer> logger) : RedisStreamConsumer<AuditLogEvent>(redis, logger)
{
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly ILogger<AuditLogConsumer> _logger = logger;

   protected override string StreamName => SentinelConst.Redis.AuditLogEventsStream;
   protected override string ConsumerGroup => SentinelConst.Redis.AuditConsumerGroup;
   protected override string ConsumerNamePrefix => SentinelConst.Redis.AuditConsumerNamePrefix;
   protected override string ConsumerDisplayName => "Audit event consumer";
   protected override string ProcessingErrorMessage => "Error processing audit event";
   protected override string ExpectedEventName => SharedConst.Event.Name.AuditLogRequested;

   protected override async Task ProcessEventAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken)
   {
      using var scope = _serviceProvider.CreateScope();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<ISentinelUnitOfWork>();

      var auditLog = AuditLog.Create(
         auditEvent.Id,
         auditEvent.Module,
         auditEvent.Feature,
         auditEvent.Action,
         auditEvent.Description,
         auditEvent.PrivacyLevel,
         auditEvent.RetentionPolicy,
         auditEvent.IpAddress,
         auditEvent.UserAgent,
         auditEvent.UserId,
         auditEvent.OrganizationId,
         auditEvent.TargetId,
         auditEvent.Metadata);

      await unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);

      _logger.LogDebug("Persisted audit event {EventId}: {Action} on {TargetId}", auditEvent.Id, auditEvent.Action, auditEvent.TargetId);
   }
}
