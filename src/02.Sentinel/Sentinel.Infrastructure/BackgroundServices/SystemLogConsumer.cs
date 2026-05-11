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

public class SystemLogConsumer(
   IConnectionMultiplexer redis,
   IServiceProvider serviceProvider,
   ILogger<SystemLogConsumer> logger) : BackgroundService
{
   private const string ConsumerGroup = "sentinel-system-log-consumer";

   private readonly IDatabase _database = redis.GetDatabase();
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly ILogger<SystemLogConsumer> _logger = logger;
   private readonly string _consumerName = $"sentinel-system-log-{Environment.MachineName}";

   private static readonly JsonSerializerOptions JsonOptions = new()
   {
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
   };

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      _logger.LogInformation("System log consumer started");
      await EnsureConsumerGroupExistsAsync();

      while (!stoppingToken.IsCancellationRequested)
      {
         try
         {
            var entries = await _database.StreamReadGroupAsync(SharedConst.Redis.SystemLogEventsStream, ConsumerGroup, _consumerName, ">", count: 10);

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
            _logger.LogError(ex, "Error consuming system logs");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
         }
      }

      _logger.LogInformation("System log consumer stopped");
   }

   private async Task EnsureConsumerGroupExistsAsync()
   {
      try
      {
         await _database.StreamCreateConsumerGroupAsync(SharedConst.Redis.SystemLogEventsStream, ConsumerGroup, "0-0", createStream: true);
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
            await _database.StreamAcknowledgeAsync(SharedConst.Redis.SystemLogEventsStream, ConsumerGroup, entry.Id);
            return;
         }

         var systemLogEvent = JsonSerializer.Deserialize<SystemLogEvent>(json.ToString(), JsonOptions);

         if (systemLogEvent == null)
         {
            await _database.StreamAcknowledgeAsync(SharedConst.Redis.SystemLogEventsStream, ConsumerGroup, entry.Id);
            return;
         }

         await ProcessEventAsync(systemLogEvent, cancellationToken);
         await _database.StreamAcknowledgeAsync(SharedConst.Redis.SystemLogEventsStream, ConsumerGroup, entry.Id);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error processing system log {EntryId}", entry.Id);
      }
   }

   private async Task ProcessEventAsync(SystemLogEvent systemLogEvent, CancellationToken cancellationToken)
   {
      using var scope = _serviceProvider.CreateScope();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<ISentinelUnitOfWork>();

      var propertiesJson = JsonSerializer.Serialize(systemLogEvent.Properties, JsonOptions);
      var systemLog = SystemLog.Create(
         systemLogEvent.Timestamp,
         systemLogEvent.Level,
         systemLogEvent.Status,
         systemLogEvent.Source,
         systemLogEvent.Message,
         systemLogEvent.Exception,
         systemLogEvent.StackTrace,
         systemLogEvent.RequestId,
         systemLogEvent.UserId,
         systemLogEvent.OrganizationId,
         propertiesJson);

      await unitOfWork.SystemLogs.AddAsync(systemLog, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);

      _logger.LogDebug("Persisted system log {LogId}: {Level} {Status}", systemLogEvent.LogId, systemLogEvent.Level, systemLogEvent.Status);
   }
}
