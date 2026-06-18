using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentinel.Domain;
using Shared.Domain;
using Shared.Domain.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace Sentinel.Infrastructure.BackgroundServices;

public abstract class RedisStreamConsumer<TEvent>(
   IConnectionMultiplexer redis,
   ILogger logger) : BackgroundService
{
   private readonly IDatabase _database = redis.GetDatabase();
   private readonly ILogger _logger = logger;

   protected abstract string StreamName { get; }
   protected abstract string ConsumerGroup { get; }
   protected abstract string ConsumerNamePrefix { get; }
   protected abstract string ConsumerDisplayName { get; }
   protected abstract string ProcessingErrorMessage { get; }
   protected abstract string ExpectedEventName { get; }
   protected virtual int ExpectedEventVersion => SharedConst.Event.Version;
   protected abstract Task ProcessEventAsync(TEvent eventData, CancellationToken cancellationToken);

   protected virtual JsonSerializerOptions JsonOptions { get; } = new()
   {
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
   };

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      _logger.LogInformation("{ConsumerDisplayName} started", ConsumerDisplayName);
      await EnsureConsumerGroupExistsAsync();

      while (!stoppingToken.IsCancellationRequested)
      {
         try
         {
            var entries = await _database.StreamReadGroupAsync(
               StreamName,
               ConsumerGroup,
               GetConsumerName(),
               ">",
               count: SentinelConst.Redis.ReadBatchSize);

            foreach (var entry in entries)
            {
               await ProcessEntryAsync(entry, stoppingToken);
            }

            if (entries.Length == 0)
            {
               await Task.Delay(TimeSpan.FromSeconds(SentinelConst.Redis.EmptyStreamDelaySeconds), stoppingToken);
            }
         }
         catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
         {
            break;
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Error consuming {ConsumerDisplayName}", ConsumerDisplayName);
            await Task.Delay(TimeSpan.FromSeconds(SentinelConst.Redis.ErrorDelaySeconds), stoppingToken);
         }
      }

      _logger.LogInformation("{ConsumerDisplayName} stopped", ConsumerDisplayName);
   }

   private async Task EnsureConsumerGroupExistsAsync()
   {
      try
      {
         await _database.StreamCreateConsumerGroupAsync(
            StreamName,
            ConsumerGroup,
            SentinelConst.Redis.InitialStreamPosition,
            createStream: true);
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
         var json = entry.Values.FirstOrDefault(v => v.Name == SentinelConst.Redis.EventFieldName).Value;

         if (json.IsNullOrEmpty)
         {
            await AcknowledgeAsync(entry);
            return;
         }

         var envelope = JsonSerializer.Deserialize<IntegrationEvent<TEvent>>(json.ToString(), JsonOptions);

         if (envelope == null)
         {
            await AcknowledgeAsync(entry);
            return;
         }

         if (envelope.EventName != ExpectedEventName || envelope.Version != ExpectedEventVersion)
         {
            _logger.LogWarning(
               "Unsupported event {EventName} version {Version} in {ConsumerDisplayName}",
               envelope.EventName,
               envelope.Version,
               ConsumerDisplayName);
            await AcknowledgeAsync(entry);
            return;
         }

         await ProcessEventAsync(envelope.Payload, cancellationToken);
         await AcknowledgeAsync(entry);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "{ProcessingErrorMessage} {EntryId}", ProcessingErrorMessage, entry.Id);
      }
   }

   private Task AcknowledgeAsync(StreamEntry entry)
   {
      return _database.StreamAcknowledgeAsync(StreamName, ConsumerGroup, entry.Id);
   }

   private string GetConsumerName()
   {
      return $"{ConsumerNamePrefix}-{Environment.MachineName}";
   }
}
