using Microsoft.Extensions.Logging;
using Shared.Domain;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Shared.Infrastructure.Messaging;

public class RedisEventPublisher(IConnectionMultiplexer redis, ILogger<RedisEventPublisher> logger) : IEventPublisher
{
   private readonly IDatabase _database = redis.GetDatabase();
   private readonly ISubscriber _subscriber = redis.GetSubscriber();
   private readonly ILogger<RedisEventPublisher> _logger = logger;

   private static readonly JsonSerializerOptions JsonOptions = new()
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
   };

   public async Task PublishAuditLogEventAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      var json = JsonSerializer.Serialize(auditEvent, JsonOptions);
      var streamId = await _database.StreamAddAsync(SharedConst.Redis.AuditLogEventsStream, "event", json);

      _logger.LogDebug("Published audit event {EventId} to stream {StreamId}", auditEvent.EventId, streamId);
   }

   public async Task PublishSystemLogEventAsync(SystemLogEvent systemLog, CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      var json = JsonSerializer.Serialize(systemLog, JsonOptions);
      var streamId = await _database.StreamAddAsync(SharedConst.Redis.SystemLogEventsStream, "event", json);

      _logger.LogDebug("Published system log {LogId} to stream {StreamId}", systemLog.LogId, streamId);
   }

   public async Task PublishNotificationEventAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      var json = JsonSerializer.Serialize(notification, JsonOptions);
      await _subscriber.PublishAsync(RedisChannel.Literal(SharedConst.Redis.NotificationEventsChannel), json);

      _logger.LogDebug("Published notification {NotificationId}", notification.NotificationId);
   }
}
