using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Sentinel.Infrastructure.BackgroundServices;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using StackExchange.Redis;

namespace Sentinel.Infrastructure.Tests.BackgroundServices;

public class SystemLogConsumerTests
{
   [Fact]
   public async Task ProcessEventAsync_ShouldPersistSystemLog()
   {
      var systemLogRepository = Substitute.For<ISystemLogRepository>();
      var unitOfWork = Substitute.For<ISentinelUnitOfWork>();
      unitOfWork.SystemLogs.Returns(systemLogRepository);

      var consumer = CreateConsumer(unitOfWork);
      var systemLogEvent = new SystemLogEvent
      {
         CreatedAt = DateTime.UtcNow,
         Level = SystemLogLevel.Error,
         Status = SystemLogStatus.Unauthorized,
         Source = "Sentinel.Tests",
         Message = "Unauthorized request",
         Exception = "exception",
         StackTrace = "stack",
         RequestId = Guid.CreateVersion7().ToString(),
         UserId = Guid.CreateVersion7(),
         OrganizationId = Guid.CreateVersion7(),
         Properties = new Dictionary<string, object>
         {
            ["path"] = "/api/users"
         }
      };

      await consumer.ProcessAsync(systemLogEvent, CancellationToken.None);

      await systemLogRepository.Received(1).AddAsync(
         Arg.Is<SystemLog>(log =>
            log.CreatedAt == systemLogEvent.CreatedAt &&
            log.Level == systemLogEvent.Level &&
            log.Status == systemLogEvent.Status &&
            log.Source == systemLogEvent.Source &&
            log.Message == systemLogEvent.Message &&
            log.Exception == systemLogEvent.Exception &&
            log.StackTrace == systemLogEvent.StackTrace &&
            log.RequestId == systemLogEvent.RequestId &&
            log.UserId == systemLogEvent.UserId &&
            log.OrganizationId == systemLogEvent.OrganizationId &&
            log.PropertiesJson.Contains("path")),
         Arg.Any<CancellationToken>());

      await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   private static TestableSystemLogConsumer CreateConsumer(ISentinelUnitOfWork unitOfWork)
   {
      var services = new ServiceCollection()
         .AddScoped(_ => unitOfWork)
         .BuildServiceProvider();

      var redis = Substitute.For<IConnectionMultiplexer>();
      redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(Substitute.For<IDatabase>());

      return new TestableSystemLogConsumer(
         redis,
         services,
         Substitute.For<ILogger<SystemLogConsumer>>());
   }

   private class TestableSystemLogConsumer(
      IConnectionMultiplexer redis,
      IServiceProvider serviceProvider,
      ILogger<SystemLogConsumer> logger) : SystemLogConsumer(redis, serviceProvider, logger)
   {
      public Task ProcessAsync(SystemLogEvent systemLogEvent, CancellationToken cancellationToken)
      {
         return ProcessEventAsync(systemLogEvent, cancellationToken);
      }
   }
}
