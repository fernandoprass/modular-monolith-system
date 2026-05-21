using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentinel.Domain;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Shared.Domain.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace Sentinel.Infrastructure.BackgroundServices;

public class SystemLogConsumer(
   IConnectionMultiplexer redis,
   IServiceProvider serviceProvider,
   ILogger<SystemLogConsumer> logger) : RedisStreamConsumer<SystemLogEvent>(redis, logger)
{
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly ILogger<SystemLogConsumer> _logger = logger;

   protected override string StreamName => SentinelConst.Redis.SystemLogEventsStream;
   protected override string ConsumerGroup => SentinelConst.Redis.SystemLogConsumerGroup;
   protected override string ConsumerNamePrefix => SentinelConst.Redis.SystemLogConsumerNamePrefix;
   protected override string ConsumerDisplayName => "System log consumer";
   protected override string ProcessingErrorMessage => "Error processing system log";

   protected override async Task ProcessEventAsync(SystemLogEvent systemLogEvent, CancellationToken cancellationToken)
   {
      using var scope = _serviceProvider.CreateScope();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<ISentinelUnitOfWork>();

      var propertiesJson = JsonSerializer.Serialize(systemLogEvent.Properties, JsonOptions);
      var systemLog = SystemLog.Create(
         systemLogEvent.Id,
         systemLogEvent.CreatedAt,
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

      _logger.LogDebug("Persisted system log {LogId}: {Level} {Status}", systemLogEvent.Id, systemLogEvent.Level, systemLogEvent.Status);
   }
}
