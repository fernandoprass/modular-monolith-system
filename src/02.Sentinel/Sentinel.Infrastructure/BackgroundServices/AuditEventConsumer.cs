using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Shared.Domain;
using Shared.Domain.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace Sentinel.Infrastructure.BackgroundServices;

public class AuditEventConsumer(
   IConnectionMultiplexer redis,
   IServiceProvider serviceProvider,
   ILogger<AuditEventConsumer> logger) : BackgroundService
{
   private const string ConsumerGroup = "sentinel-audit-consumer";

   private readonly IDatabase _database = redis.GetDatabase();
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly ILogger<AuditEventConsumer> _logger = logger;
   private readonly string _consumerName = $"sentinel-audit-{Environment.MachineName}";

   private static readonly JsonSerializerOptions JsonOptions = new()
   {
      PropertyNameCaseInsensitive = true
   };

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      _logger.LogInformation("Audit event consumer started");
      await EnsureConsumerGroupExistsAsync();

      while (!stoppingToken.IsCancellationRequested)
      {
         try
         {
            var entries = await _database.StreamReadGroupAsync(SharedConst.Redis.AuditLogEventsStream, ConsumerGroup, _consumerName, ">", count: 10);

            foreach (var entry in entries)
            {
               await ProcessEntryAsync(entry, stoppingToken);
            }

            if (entries.Length == 0)
            {
               await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
         }
         catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
         {
            break;
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Error consuming audit events");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
         }
      }

      _logger.LogInformation("Audit event consumer stopped");
   }

   private async Task EnsureConsumerGroupExistsAsync()
   {
      try
      {
         await _database.StreamCreateConsumerGroupAsync(SharedConst.Redis.AuditLogEventsStream, ConsumerGroup, "0-0", createStream: true);
      }
      catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
      {
         _logger.LogDebug("Consumer group {ConsumerGroup} already exists", ConsumerGroup);
      }
   }

   private async Task ProcessEntryAsync(StreamEntry entry, CancellationToken cancellationToken)
   {
      try
      {
         var json = entry.Values.FirstOrDefault(v => v.Name == "event").Value;

         if (json.IsNullOrEmpty)
         {
            await _database.StreamAcknowledgeAsync(SharedConst.Redis.AuditLogEventsStream, ConsumerGroup, entry.Id);
            return;
         }

         var auditEvent = JsonSerializer.Deserialize<AuditLogEvent>(json.ToString(), JsonOptions);

         if (auditEvent == null)
         {
            await _database.StreamAcknowledgeAsync(SharedConst.Redis.AuditLogEventsStream, ConsumerGroup, entry.Id);
            return;
         }

         await ProcessEventAsync(auditEvent, cancellationToken);
         await _database.StreamAcknowledgeAsync(SharedConst.Redis.AuditLogEventsStream, ConsumerGroup, entry.Id);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error processing audit event {EntryId}", entry.Id);
      }
   }

   private async Task ProcessEventAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken)
   {
      using var scope = _serviceProvider.CreateScope();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<ISentinelUnitOfWork>();

      var auditLog = AuditLog.Create(
         auditEvent.Timestamp,
         auditEvent.Module,
         auditEvent.Feature,
         auditEvent.Action,
         auditEvent.PrivacyLevel,
         auditEvent.Description,
         auditEvent.UserId,
         auditEvent.OrganizationId,
         auditEvent.Entity,
         auditEvent.EntityId,
         auditEvent.IpAddress,
         auditEvent.UserAgent,
         auditEvent.Metadata);

      await unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);

      _logger.LogDebug("Persisted audit event {EventId}: {Action} on {Entity}", auditEvent.EventId, auditEvent.Action, auditEvent.Entity);
   }
}
