using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Infrastructure.BackgroundServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Enums;

namespace Courier.Infrastructure.Tests.BackgroundServices;

public class EmailDeliveryWorkerTests
{
   [Fact]
   public async Task ProcessBatchAsync_ShouldProcessUntilBatchSize()
   {
      var outboxService = Substitute.For<IEmailOutboxService>();
      var worker = CreateWorker(outboxService);
      outboxService.ProcessNextPendingAsync(Arg.Any<CancellationToken>())
         .Returns(_ => Task.FromResult(true));

      var processed = await worker.ProcessBatchAsync(TestContext.Current.CancellationToken);

      processed.Should().BeTrue();
      await outboxService.Received(CourierConst.Worker.EmailDeliveryBatchSize)
         .ProcessNextPendingAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task ProcessBatchAsync_ShouldStop_WhenNoPendingEmailExists()
   {
      var outboxService = Substitute.For<IEmailOutboxService>();
      var worker = CreateWorker(outboxService);
      outboxService.ProcessNextPendingAsync(Arg.Any<CancellationToken>())
         .Returns(Task.FromResult(false));

      var processed = await worker.ProcessBatchAsync(TestContext.Current.CancellationToken);

      processed.Should().BeFalse();
      await outboxService.Received(1).ProcessNextPendingAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task ProcessBatchAsync_ShouldReturnTrue_WhenAtLeastOneEmailIsProcessed()
   {
      var outboxService = Substitute.For<IEmailOutboxService>();
      var worker = CreateWorker(outboxService);
      outboxService.ProcessNextPendingAsync(Arg.Any<CancellationToken>())
         .Returns(Task.FromResult(true), Task.FromResult(false));

      var processed = await worker.ProcessBatchAsync(TestContext.Current.CancellationToken);

      processed.Should().BeTrue();
      await outboxService.Received(2).ProcessNextPendingAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task TryLogSystemErrorAsync_ShouldLogSystemError()
   {
      var outboxService = Substitute.For<IEmailOutboxService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var worker = CreateWorker(outboxService, courierLogger);
      var exception = new InvalidOperationException("send failed");

      await worker.TryLogSystemErrorAsync(exception, TestContext.Current.CancellationToken);

      await courierLogger.Received(1).LogSystemAsync(
         SystemLogLevel.Error,
         SystemLogStatus.Failure,
         "Courier email delivery worker failed",
         exception,
         null,
         null,
         null,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task TryLogSystemErrorAsync_ShouldNotThrow_WhenSystemLogFails()
   {
      var outboxService = Substitute.For<IEmailOutboxService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var worker = CreateWorker(outboxService, courierLogger);
      courierLogger.LogSystemAsync(
            Arg.Any<SystemLogLevel>(),
            Arg.Any<SystemLogStatus>(),
            Arg.Any<string>(),
            Arg.Any<Exception>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<Dictionary<string, object>?>(),
            Arg.Any<CancellationToken>())
         .Returns<Task>(_ => throw new InvalidOperationException("log failed"));

      var act = () => worker.TryLogSystemErrorAsync(
         new InvalidOperationException("send failed"),
         TestContext.Current.CancellationToken);

      await act.Should().NotThrowAsync();
   }

   private static EmailDeliveryWorker CreateWorker(
      IEmailOutboxService outboxService,
      ICourierLogger? courierLogger = null)
   {
      var services = new ServiceCollection()
         .AddScoped(_ => outboxService)
         .AddScoped(_ => courierLogger ?? Substitute.For<ICourierLogger>())
         .BuildServiceProvider();

      return new EmailDeliveryWorker(
         services,
         Substitute.For<ILogger<EmailDeliveryWorker>>());
   }
}
