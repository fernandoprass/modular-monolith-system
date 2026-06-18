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

      var httpContext = CreateHttpContext();

      httpContext.TraceIdentifier = "request-1";
      httpContext.Response.StatusCode = statusCode;

      var logEvent = SystemLogEventFactory.Create("IAM", httpContext, exception, userContext);

      Assert.Equal(SystemLogLevel.Error, logEvent.Level);
      Assert.Equal(expectedStatus, logEvent.Status);
      Assert.Equal(expectedPolicy, logEvent.RetentionPolicy);
      Assert.Equal("IAM", logEvent.Module);
      Assert.Equal(exception.Message, logEvent.Message);
      Assert.Equal(exception.GetType().Name, logEvent.Exception);
      Assert.Equal("request-1", logEvent.RequestId);
      Assert.Equal(userId, logEvent.UserId);
      Assert.Equal(organizationId, logEvent.OrganizationId);
      Assert.Equal("/api/test", logEvent.Properties["path"]);
      Assert.Equal(statusCode, logEvent.Properties["statusCode"]);
   }

   [Fact]
   public void Create_ShouldIncludeSafeRequestProperties()
   {
      var userContext = new FakeUserContext(Guid.CreateVersion7(), Guid.CreateVersion7());
      var httpContext = TestHttpContextFactory.Create(
         statusCode: 500,
         requestId: "request-1",
         queryKeys: ["token", "page"],
         contentType: "application/json",
         contentLength: 42);

      var logEvent = SystemLogEventFactory.Create("IAM", httpContext, new InvalidOperationException("Boom"), userContext);

      Assert.Equal(["token", "page"], Assert.IsType<string[]>(logEvent.Properties["queryKeys"]));
      Assert.Equal("application/json", logEvent.Properties["contentType"]);
      Assert.Equal(42L, logEvent.Properties["contentLength"]);
      Assert.False(logEvent.Properties.ContainsKey("queryString"));
   }

   [Fact]
   public void Create_ShouldSetNullUserValues_WhenUserContextHasEmptyIds()
   {
      var userContext = new FakeUserContext(Guid.Empty, Guid.Empty);

      var httpContext = CreateHttpContext();
      httpContext.TraceIdentifier = "request-1";
      httpContext.Response.StatusCode = 500;

      var logEvent = SystemLogEventFactory.Create("IAM", httpContext, new InvalidOperationException("Boom"), userContext);

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

   private static HttpContext CreateHttpContext()
   {
      return TestHttpContextFactory.Create();
   }

   private class FakeUserContext(Guid userId, Guid organizationId) : IUserContext
   {
      public Guid UserId { get; } = userId;
      public Guid OrganizationId { get; } = organizationId;
      public bool IsSystemAdmin => false;
      public bool IsOrganizationAdmin => false;
      public bool IsAuthenticated => true;
      public string? IpAddress => "127.0.0.1";
      public string? UserAgent => "test-agent";
      public string Language => "en";
      public IEnumerable<string> Roles => [];
   }
}
