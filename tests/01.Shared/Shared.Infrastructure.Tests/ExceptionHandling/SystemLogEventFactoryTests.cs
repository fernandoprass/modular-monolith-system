using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class SystemLogEventFactoryTests
{
   [Theory]
   [InlineData("unauthorized", 401, SystemLogStatus.Unauthorized, RetentionPolicy.Extended)]
   [InlineData("generic", 500, SystemLogStatus.Failure, RetentionPolicy.Operational)]
   public void Create_ShouldMapExceptionToSystemLogEvent(
      string exceptionType,
      int statusCode,
      SystemLogStatus expectedStatus,
      RetentionPolicy expectedPolicy)
   {
      var userId = Guid.CreateVersion7();
      var organizationId = Guid.CreateVersion7();
      var userContext = new FakeUserContext(userId, organizationId);
      var exception = CreateException(exceptionType);

      var request = CreateRequest();

      var logEvent = SystemLogEventFactory.Create("IAM", request, exception, statusCode, "request-1", userContext);

      Assert.Equal(SystemLogLevel.Error, logEvent.Level);
      Assert.Equal(expectedStatus, logEvent.Status);
      Assert.Equal(expectedPolicy, logEvent.RetentionPolicy);
      Assert.Equal("IAM", logEvent.Source);
      Assert.Equal(exception.Message, logEvent.Message);
      Assert.Equal(exception.GetType().Name, logEvent.Exception);
      Assert.Equal("request-1", logEvent.RequestId);
      Assert.Equal(userId, logEvent.UserId);
      Assert.Equal(organizationId, logEvent.OrganizationId);
      Assert.Equal("/api/test", logEvent.Properties["path"]);
      Assert.Equal(statusCode, logEvent.Properties["statusCode"]);
   }

   [Fact]
   public void Create_ShouldSetNullUserValues_WhenUserContextHasEmptyIds()
   {
      var userContext = new FakeUserContext(Guid.Empty, Guid.Empty);

      var request = CreateRequest();

      var logEvent = SystemLogEventFactory.Create("IAM", request, new InvalidOperationException("Boom"), 500, "request-1", userContext);

      Assert.Null(logEvent.UserId);
      Assert.Null(logEvent.OrganizationId);
   }

   private static Exception CreateException(string exceptionType)
   {
      return exceptionType switch
      {
         "unauthorized" => new UnauthorizedAccessException("Denied"),
         _ => new InvalidOperationException("Boom")
      };
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

   private class FakeUserContext(Guid userId, Guid userOwnerId) : IUserContext
   {
      public Guid UserId { get; } = userId;
      public Guid UserOwnerId { get; } = userOwnerId;
      public bool IsSystemAdmin => false;
      public bool IsOrganizationAdmin => false;
      public bool IsAuthenticated => true;
      public string? IpAddress => "127.0.0.1";
      public string? UserAgent => "test-agent";
      public string Language => "en";
      public IEnumerable<string> Roles => [];
   }
}
