using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class ExceptionSystemLogPublisherTests
{
   [Fact]
   public async Task PublishAsync_ShouldPublishSystemLogEvent()
   {
      var eventPublisher = Substitute.For<IEventPublisher>();
      var userContext = Substitute.For<IUserContext>();
      userContext.UserId.Returns(Guid.CreateVersion7());
      userContext.UserOwnerId.Returns(Guid.CreateVersion7());
      var publisher = new ExceptionSystemLogPublisher(eventPublisher, userContext, Substitute.For<ILogger<ExceptionSystemLogPublisher>>());

      await publisher.PublishAsync(
         "IAM",
         new InvalidOperationException("Boom"),
         500,
         "request-id",
         "/api/test",
         TestContext.Current.CancellationToken);

      await eventPublisher.Received(1).PublishSystemLogEventAsync(
         Arg.Is<SystemLogEvent>(log => log.Source == "IAM" && log.Message == "Boom"),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task PublishAsync_ShouldNotThrow_WhenPublishFails()
   {
      var eventPublisher = Substitute.For<IEventPublisher>();
      eventPublisher.PublishSystemLogEventAsync(Arg.Any<SystemLogEvent>(), Arg.Any<CancellationToken>())
         .Returns(_ => throw new InvalidOperationException("Redis unavailable"));
      var publisher = new ExceptionSystemLogPublisher(eventPublisher, Substitute.For<IUserContext>(), Substitute.For<ILogger<ExceptionSystemLogPublisher>>());

      var act = async () => await publisher.PublishAsync(
         "IAM",
         new InvalidOperationException("Boom"),
         500,
         "request-id",
         "/api/test",
         TestContext.Current.CancellationToken);

      await act();
   }
}
