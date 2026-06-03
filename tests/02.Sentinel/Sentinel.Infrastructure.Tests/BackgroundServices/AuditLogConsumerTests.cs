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

public class AuditLogConsumerTests
{
   [Fact]
   public async Task ProcessEventAsync_ShouldPersistAuditLog()
   {
      var auditLogRepository = Substitute.For<IAuditLogRepository>();
      var unitOfWork = Substitute.For<ISentinelUnitOfWork>();
      unitOfWork.AuditLogs.Returns(auditLogRepository);

      var consumer = CreateConsumer(unitOfWork);
      var auditEvent = AuditLogEvent.Create(
         module : "iam",
         feature : "users",
         action : "create",
         description: "Created user",
         privacyLevel : AuditPrivacyLevel.Confidential,
         retentionPolicy: RetentionPolicy.Standard,
         ipAddress: "127.0.0.1",
         userAgent: "test-agent",
         userId : Guid.CreateVersion7(),
         targetId : Guid.CreateVersion7(),
         organizationId: Guid.CreateVersion7(),
         metadata : "{\"name\":\"Test\"}"
      );

      await consumer.ProcessAsync(auditEvent, CancellationToken.None);

      await auditLogRepository.Received(1).AddAsync(
         Arg.Is<AuditLog>(log =>
            log.Module == auditEvent.Module &&
            log.Feature == auditEvent.Feature &&
            log.Action == auditEvent.Action &&
            log.PrivacyLevel == auditEvent.PrivacyLevel &&
            log.Description == auditEvent.Description &&
            log.UserId == auditEvent.UserId &&
            log.OrganizationId == auditEvent.OrganizationId &&
            log.TargetId == auditEvent.TargetId &&
            log.IpAddress == auditEvent.IpAddress &&
            log.UserAgent == auditEvent.UserAgent &&
            log.Metadata == auditEvent.Metadata),
         Arg.Any<CancellationToken>());

      await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   private static TestableAuditLogConsumer CreateConsumer(ISentinelUnitOfWork unitOfWork)
   {
      var services = new ServiceCollection()
         .AddScoped(_ => unitOfWork)
         .BuildServiceProvider();

      var redis = Substitute.For<IConnectionMultiplexer>();
      redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(Substitute.For<IDatabase>());

      return new TestableAuditLogConsumer(
         redis,
         services,
         Substitute.For<ILogger<AuditLogConsumer>>());
   }

   private class TestableAuditLogConsumer(
      IConnectionMultiplexer redis,
      IServiceProvider serviceProvider,
      ILogger<AuditLogConsumer> logger) : AuditLogConsumer(redis, serviceProvider, logger)
   {
      public Task ProcessAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken)
      {
         return ProcessEventAsync(auditEvent, cancellationToken);
      }
   }
}
