using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class SystemLogEventFactoryTests
{
   [Theory]
   [InlineData(401, SystemLogStatus.Unauthorized)]
   [InlineData(500, SystemLogStatus.Failure)]
   public void Create_ShouldMapExceptionToSystemLogEvent(int statusCode, SystemLogStatus expectedStatus)
   {
      var userId = Guid.CreateVersion7();
      var organizationId = Guid.CreateVersion7();
      var userContext = new FakeUserContext(userId, organizationId);
      var exception = new InvalidOperationException("Boom");

      var logEvent = SystemLogEventFactory.Create("IAM", exception, statusCode, "request-1", "/api/test", userContext);

      Assert.Equal(SystemLogLevel.Error, logEvent.Level);
      Assert.Equal(expectedStatus, logEvent.Status);
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

      var logEvent = SystemLogEventFactory.Create("IAM", new InvalidOperationException("Boom"), 500, "request-1", "/api/test", userContext);

      Assert.Null(logEvent.UserId);
      Assert.Null(logEvent.OrganizationId);
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
