using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace Courier.Infrastructure.BackgroundServices;

public class MessageRequestConsumer(
   IConnectionMultiplexer redis,
   IServiceProvider serviceProvider,
   ILogger<MessageRequestConsumer> logger) : BackgroundService
{
   private readonly IDatabase _database = redis.GetDatabase();
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly ILogger<MessageRequestConsumer> _logger = logger;

   private static readonly JsonSerializerOptions JsonOptions = new()
   {
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
   };

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      _logger.LogInformation("Courier message request consumer started");
      await EnsureConsumerGroupExistsAsync();

      while (!stoppingToken.IsCancellationRequested)
      {
         try
         {
            var entries = await _database.StreamReadGroupAsync(
               CourierConst.Redis.MessageRequestsStream,
               CourierConst.Redis.MessageRequestConsumerGroup,
               GetConsumerName(),
               CourierConst.Redis.NewMessagesStreamPosition,
               count: CourierConst.Redis.ReadBatchSize);

            foreach (var entry in entries)
            {
               await ProcessEntryAsync(entry, stoppingToken);
            }

            if (entries.Length == 0)
            {
               await Task.Delay(TimeSpan.FromSeconds(CourierConst.Redis.EmptyStreamDelaySeconds), stoppingToken);
            }
         }
         catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
         {
            break;
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Error consuming Courier message requests");
            await TryLogSystemErrorAsync("Error consuming Courier message requests", ex, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(CourierConst.Redis.ErrorDelaySeconds), stoppingToken);
         }
      }

      _logger.LogInformation("Courier message request consumer stopped");
   }

   private async Task EnsureConsumerGroupExistsAsync()
   {
      try
      {
         await _database.StreamCreateConsumerGroupAsync(
            CourierConst.Redis.MessageRequestsStream,
            CourierConst.Redis.MessageRequestConsumerGroup,
            CourierConst.Redis.InitialStreamPosition,
            createStream: true);
      }
      catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
      {
         _logger.LogDebug("Consumer group {ConsumerGroup} already exists", CourierConst.Redis.MessageRequestConsumerGroup);
      }
   }

   internal async Task ProcessEntryAsync(StreamEntry entry, CancellationToken cancellationToken)
   {
      try
      {
         var json = entry.Values.FirstOrDefault(v => v.Name == CourierConst.Redis.EventFieldName).Value;

         if (json.IsNullOrEmpty)
         {
            await AcknowledgeAsync(entry);
            return;
         }

         IntegrationEvent<CourierMessageRequest>? envelope;

         try
         {
            envelope = JsonSerializer.Deserialize<IntegrationEvent<CourierMessageRequest>>(json.ToString(), JsonOptions);
         }
         catch (JsonException ex)
         {
            _logger.LogError(ex, "Invalid Courier message request {EntryId}", entry.Id);
            await TryLogSystemErrorAsync($"Invalid Courier message request {entry.Id}", ex, cancellationToken);
            await AcknowledgeAsync(entry);
            return;
         }
         catch (NotSupportedException ex)
         {
            _logger.LogError(ex, "Unsupported Courier message request {EntryId}", entry.Id);
            await TryLogSystemErrorAsync($"Unsupported Courier message request {entry.Id}", ex, cancellationToken);
            await AcknowledgeAsync(entry);
            return;
         }

         if (envelope == null)
         {
            await AcknowledgeAsync(entry);
            return;
         }

         if (envelope.EventName != CourierConst.Event.Name.MessageRequested || envelope.Version != CourierConst.Event.Version)
         {
            _logger.LogWarning(
               "Unsupported Courier message request event {EventName} version {Version}",
               envelope.EventName,
               envelope.Version);
            await AcknowledgeAsync(entry);
            return;
         }

         var request = envelope.Payload;

         if (request == null)
         {
            await TryLogQueueFailureAsync(entry, "Courier message request payload is missing", null, cancellationToken);
            await AcknowledgeAsync(entry);
            return;
         }

         using var scope = _serviceProvider.CreateScope();
         var service = scope.ServiceProvider.GetRequiredService<ICourierMessageService>();
         var result = await service.QueueAsync(request, cancellationToken);

         if (result.HasError)
         {
            await TryLogQueueFailureAsync(
               entry,
               $"Failed to queue Courier message request {entry.Id}",
               request,
               cancellationToken,
               new KeyValuePair<string, object>("errors", string.Join(" | ", result.Messages.Select(m => m.Show()))));
         }

         await AcknowledgeAsync(entry);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Failed to process Courier message request {EntryId}", entry.Id);
         await TryLogSystemErrorAsync($"Failed to process Courier message request {entry.Id}", ex, cancellationToken);
      }
   }

   private Task<long> AcknowledgeAsync(StreamEntry entry)
   {
      return _database.StreamAcknowledgeAsync(CourierConst.Redis.MessageRequestsStream, CourierConst.Redis.MessageRequestConsumerGroup, entry.Id);
   }

   private static string GetConsumerName()
   {
      return $"{CourierConst.Redis.MessageRequestConsumerNamePrefix}-{Environment.MachineName}";
   }

   private async Task TryLogQueueFailureAsync(
      StreamEntry entry,
      string message,
      CourierMessageRequest? request,
      CancellationToken cancellationToken,
      params KeyValuePair<string, object>[] extraProperties)
   {
      try
      {
         var properties = new Dictionary<string, object>
         {
            ["streamId"] = entry.Id.ToString()
         };

         if (request != null)
         {
            properties["templateKey"] = request.TemplateKey;
            properties["recipient"] = request.Recipient ?? string.Empty;
         }

         foreach (var property in extraProperties)
         {
            properties[property.Key] = property.Value;
         }

         using var scope = _serviceProvider.CreateScope();
         var logger = scope.ServiceProvider.GetRequiredService<ICourierLogger>();
         await logger.LogSystemAsync(
            SystemLogLevel.Warning,
            SystemLogStatus.Failure,
            message,
            null,
            request?.OrganizationId,
            request?.UserId,
            properties,
            cancellationToken);
      }
      catch (Exception logException)
      {
         _logger.LogError(logException, "Failed to log Courier queue failure");
      }
   }

   private async Task TryLogSystemErrorAsync(string message, Exception exception, CancellationToken cancellationToken)
   {
      try
      {
         using var scope = _serviceProvider.CreateScope();
         var logger = scope.ServiceProvider.GetRequiredService<ICourierLogger>();
         await logger.LogSystemAsync(SystemLogLevel.Error, SystemLogStatus.Failure, message, exception, cancellationToken: cancellationToken);
      }
      catch (Exception logException)
      {
         _logger.LogError(logException, "Failed to log Courier system error");
      }
   }
}
