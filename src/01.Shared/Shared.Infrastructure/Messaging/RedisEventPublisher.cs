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

   public async Task PublishAuditLogEventAsync(AuditLogEvent auditLogEvent, CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      var envelope = IntegrationEvent<AuditLogEvent>.Create(
         SharedConst.Event.Name.AuditLogRequested,
         SharedConst.Event.Version,
         auditLogEvent);
      var json = JsonSerializer.Serialize(envelope, JsonOptions);
      var streamId = await _database.StreamAddAsync(SharedConst.Redis.AuditLogEventsStream, "event", json);

      _logger.LogDebug("Published audit event {EventId} to stream {StreamId}", auditLogEvent.Id, streamId);
   }

   public async Task PublishSystemLogEventAsync(SystemLogEvent systemLogEvent, CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      var envelope = IntegrationEvent<SystemLogEvent>.Create(
         SharedConst.Event.Name.SystemLogRequested,
         SharedConst.Event.Version,
         systemLogEvent);
      var json = JsonSerializer.Serialize(envelope, JsonOptions);
      var streamId = await _database.StreamAddAsync(SharedConst.Redis.SystemLogEventsStream, "event", json);

      _logger.LogDebug("Published system log {EventId} to stream {StreamId}", systemLogEvent.Id, streamId);
   }

   public async Task PublishNotificationEventAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();

      var envelope = IntegrationEvent<NotificationEvent>.Create(
         SharedConst.Event.Name.NotificationRequested,
         SharedConst.Event.Version,
         notification);
      var json = JsonSerializer.Serialize(envelope, JsonOptions);
      await _subscriber.PublishAsync(RedisChannel.Literal(SharedConst.Redis.NotificationEventsChannel), json);

      _logger.LogDebug("Published notification {NotificationId}", notification.Id);
   }
}
