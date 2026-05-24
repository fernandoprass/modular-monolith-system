using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Events;
using Shared.Domain.Enums;
using StackExchange.Redis;
using System.Text.Json;

namespace Courier.Infrastructure.BackgroundServices;

public class EmailRequestConsumer(
   IConnectionMultiplexer redis,
   IServiceProvider serviceProvider,
   ILogger<EmailRequestConsumer> logger) : BackgroundService
{
   private readonly IDatabase _database = redis.GetDatabase();
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly ILogger<EmailRequestConsumer> _logger = logger;

   private static readonly JsonSerializerOptions JsonOptions = new()
   {
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
   };

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      _logger.LogInformation("Courier email request consumer started");
      await EnsureConsumerGroupExistsAsync();

      while (!stoppingToken.IsCancellationRequested)
      {
         try
         {
            var entries = await _database.StreamReadGroupAsync(
               CourierConst.Redis.EmailRequestsStream,
               CourierConst.Redis.EmailRequestConsumerGroup,
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
            _logger.LogError(ex, "Error consuming Courier email requests");
            await LogSystemErrorAsync("Error consuming Courier email requests", ex, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(CourierConst.Redis.ErrorDelaySeconds), stoppingToken);
         }
      }

      _logger.LogInformation("Courier email request consumer stopped");
   }

   private async Task EnsureConsumerGroupExistsAsync()
   {
      try
      {
         await _database.StreamCreateConsumerGroupAsync(
            CourierConst.Redis.EmailRequestsStream,
            CourierConst.Redis.EmailRequestConsumerGroup,
            CourierConst.Redis.InitialStreamPosition,
            createStream: true);
      }
      catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
      {
         _logger.LogDebug("Consumer group {ConsumerGroup} already exists", CourierConst.Redis.EmailRequestConsumerGroup);
      }
   }

   private async Task ProcessEntryAsync(StreamEntry entry, CancellationToken cancellationToken)
   {
      try
      {
         var json = entry.Values.FirstOrDefault(v => v.Name == CourierConst.Redis.EventFieldName).Value;

         if (json.IsNullOrEmpty)
         {
            await AcknowledgeAsync(entry);
            return;
         }

         IntegrationEvent<EmailQueueRequest>? envelope;

         try
         {
            envelope = JsonSerializer.Deserialize<IntegrationEvent<EmailQueueRequest>>(json.ToString(), JsonOptions);
         }
         catch (JsonException ex)
         {
            _logger.LogError(ex, "Invalid Courier email request {EntryId}", entry.Id);
            await LogSystemErrorAsync($"Invalid Courier email request {entry.Id}", ex, cancellationToken);
            await AcknowledgeAsync(entry);
            return;
         }
         catch (NotSupportedException ex)
         {
            _logger.LogError(ex, "Unsupported Courier email request {EntryId}", entry.Id);
            await LogSystemErrorAsync($"Unsupported Courier email request {entry.Id}", ex, cancellationToken);
            await AcknowledgeAsync(entry);
            return;
         }

         if (envelope == null)
         {
            await AcknowledgeAsync(entry);
            return;
         }

         if (envelope.EventName != CourierConst.Event.Name.EmailRequested || envelope.Version != CourierConst.Event.Version)
         {
            _logger.LogWarning(
               "Unsupported Courier email request event {EventName} version {Version}",
               envelope.EventName,
               envelope.Version);
            await AcknowledgeAsync(entry);
            return;
         }

         var request = envelope.Payload;

         using var scope = _serviceProvider.CreateScope();
         var service = scope.ServiceProvider.GetRequiredService<IEmailOutboxService>();
         var result = await service.QueueAsync(request, cancellationToken);

         if (result.HasError)
         {
            var logger = scope.ServiceProvider.GetRequiredService<ICourierLogger>();
            await logger.LogSystemAsync(
               SystemLogLevel.Warning,
               SystemLogStatus.Failure,
               $"Failed to queue email request {entry.Id}",
               null,
               request.OrganizationId,
               request.UserId,
               new Dictionary<string, object>
               {
                  ["streamId"] = entry.Id.ToString(),
                  ["templateKey"] = request.TemplateKey,
                  ["recipient"] = request.Recipient,
                  ["errors"] = string.Join(" | ", result.Messages.Select(m => m.Show()))
               },
               cancellationToken);
         }

         await AcknowledgeAsync(entry);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Failed to process Courier email request {EntryId}", entry.Id);
         await LogSystemErrorAsync($"Failed to process Courier email request {entry.Id}", ex, cancellationToken);
      }
   }

   private Task AcknowledgeAsync(StreamEntry entry)
   {
      return _database.StreamAcknowledgeAsync(CourierConst.Redis.EmailRequestsStream, CourierConst.Redis.EmailRequestConsumerGroup, entry.Id);
   }

   private string GetConsumerName()
   {
      return $"{CourierConst.Redis.EmailRequestConsumerNamePrefix}-{Environment.MachineName}";
   }

   private async Task LogSystemErrorAsync(string message, Exception exception, CancellationToken cancellationToken)
   {
      using var scope = _serviceProvider.CreateScope();
      var logger = scope.ServiceProvider.GetRequiredService<ICourierLogger>();
      await logger.LogSystemAsync(SystemLogLevel.Error, SystemLogStatus.Failure, message, exception, cancellationToken: cancellationToken);
   }
}
