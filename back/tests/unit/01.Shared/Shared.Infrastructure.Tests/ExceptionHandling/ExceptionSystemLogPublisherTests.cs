using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
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
      userContext.OrganizationId.Returns(Guid.CreateVersion7());
      var publisher = new ExceptionSystemLogPublisher(eventPublisher, userContext, Substitute.For<ILogger<ExceptionSystemLogPublisher>>());

      await publisher.PublishAsync(
         "IAM",
         CreateHttpContext(),
         new InvalidOperationException("Boom"),
         TestContext.Current.CancellationToken);

      await eventPublisher.Received(1).PublishSystemLogEventAsync(
         Arg.Is<SystemLogEvent>(log =>
            log.Module == "IAM" &&
            log.Message == "Boom" &&
            log.Exception == nameof(InvalidOperationException) &&
            log.RequestId == "request-id" &&
            log.Status == SystemLogStatus.Failure &&
            log.RetentionPolicy == RetentionPolicy.Operational &&
            log.Properties["path"].Equals("/api/test") &&
            log.Properties["statusCode"].Equals(500)),
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
         CreateHttpContext(),
         new InvalidOperationException("Boom"),
         TestContext.Current.CancellationToken);

      await act();
   }

   private static HttpContext CreateHttpContext()
   {
      return TestHttpContextFactory.Create(statusCode: 500, requestId: "request-id");
   }
}
