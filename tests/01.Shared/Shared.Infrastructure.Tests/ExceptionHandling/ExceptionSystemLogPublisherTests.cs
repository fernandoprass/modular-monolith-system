using Microsoft.AspNetCore.Http;
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
         CreateRequest(),
         new InvalidOperationException("Boom"),
         500,
         "request-id",
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
         CreateRequest(),
         new InvalidOperationException("Boom"),
         500,
         "request-id",
         TestContext.Current.CancellationToken);

      await act();
   }

   private static HttpRequest CreateRequest()
   {
      var request = Substitute.For<HttpRequest>();
      request.Method.Returns(HttpMethods.Get);
      request.Path.Returns(new PathString("/api/test"));
      request.Scheme.Returns("https");
      request.Host.Returns(new HostString("localhost"));
      request.QueryString.Returns(QueryString.Empty);
      var query = Substitute.For<IQueryCollection>();
      query.Count.Returns(0);
      query.Keys.Returns([]);
      request.Query.Returns(query);

      return request;
   }
}
