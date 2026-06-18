using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using Shared.Domain;
using Shared.Infrastructure.Security;
using System.Net;
using System.Security.Claims;

namespace Shared.Infrastructure.Tests.Security;

public class AspNetUserContextTests
{
   [Fact]
   public void Properties_WhenUserHasValidClaims_ShouldReturnUserContextValues()
   {
      var userId = Guid.NewGuid();
      var organizationId = Guid.NewGuid();
      var httpContext = CreateHttpContext(
      [
         new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
         new Claim(SharedConst.Security.Claim.OrganizationId, organizationId.ToString()),
         new Claim(SharedConst.Security.Claim.IsSystemAdmin, "true"),
         new Claim(SharedConst.Security.Claim.IsOrganizationAdmin, "true"),
         new Claim(SharedConst.Security.Claim.Language, "es"),
         new Claim(SharedConst.Security.Claim.Role, "admin"),
         new Claim(SharedConst.Security.Claim.Role, "manager")
      ]);
      httpContext.Connection.RemoteIpAddress.Returns(IPAddress.Parse("127.0.0.1"));
      httpContext.Request.Headers[HeaderNames.UserAgent].Returns(new StringValues("UnitTest"));

      var userContext = CreateUserContext(httpContext);

      userContext.IsAuthenticated.Should().BeTrue();
      userContext.UserId.Should().Be(userId);
      userContext.OrganizationId.Should().Be(organizationId);
      userContext.IsSystemAdmin.Should().BeTrue();
      userContext.IsOrganizationAdmin.Should().BeTrue();
      userContext.Language.Should().Be("es");
      userContext.Roles.Should().BeEquivalentTo(["admin", "manager"]);
      userContext.IpAddress.Should().Be("127.0.0.1");
      userContext.UserAgent.Should().Be("UnitTest");
   }

   [Fact]
   public void UserId_WhenNameIdentifierIsMissing_ShouldUseSubjectClaim()
   {
      var userId = Guid.NewGuid();
      var httpContext = CreateHttpContext([new Claim("sub", userId.ToString())]);

      var userContext = CreateUserContext(httpContext);

      userContext.UserId.Should().Be(userId);
   }

   [Fact]
   public void Properties_WhenClaimsAreMissingOrInvalid_ShouldReturnSafeDefaults()
   {
      var httpContext = CreateHttpContext(
      [
         new Claim(ClaimTypes.NameIdentifier, "invalid-user-id"),
         new Claim(SharedConst.Security.Claim.OrganizationId, "invalid-owner-id"),
         new Claim(SharedConst.Security.Claim.IsSystemAdmin, "not-bool"),
         new Claim(SharedConst.Security.Claim.IsOrganizationAdmin, "not-bool")
      ]);

      var userContext = CreateUserContext(httpContext);

      userContext.UserId.Should().Be(Guid.Empty);
      userContext.OrganizationId.Should().Be(Guid.Empty);
      userContext.IsSystemAdmin.Should().BeFalse();
      userContext.IsOrganizationAdmin.Should().BeFalse();
      userContext.Language.Should().Be(SharedConst.System.DefaultLanguage);
      userContext.Roles.Should().BeEmpty();
   }

   [Fact]
   public void Properties_WhenHttpContextIsMissing_ShouldReturnSafeDefaults()
   {
      var accessor = new TestHttpContextAccessor(null);
      var userContext = new AspNetUserContext(accessor);

      userContext.IsAuthenticated.Should().BeFalse();
      userContext.UserId.Should().Be(Guid.Empty);
      userContext.OrganizationId.Should().Be(Guid.Empty);
      userContext.IsSystemAdmin.Should().BeFalse();
      userContext.IsOrganizationAdmin.Should().BeFalse();
      userContext.Language.Should().Be(SharedConst.System.DefaultLanguage);
      userContext.Roles.Should().BeEmpty();
      userContext.IpAddress.Should().BeNull();
      userContext.UserAgent.Should().BeNull();
   }

   private static AspNetUserContext CreateUserContext(HttpContext httpContext)
   {
      return new AspNetUserContext(new TestHttpContextAccessor(httpContext));
   }

   private static HttpContext CreateHttpContext(IEnumerable<Claim> claims)
   {
      var httpContext = Substitute.For<HttpContext>();
      var request = Substitute.For<HttpRequest>();
      var connection = Substitute.For<ConnectionInfo>();
      var headers = Substitute.For<IHeaderDictionary>();

      request.Headers.Returns(headers);
      headers[HeaderNames.UserAgent].Returns(StringValues.Empty);
      httpContext.Request.Returns(request);
      httpContext.Connection.Returns(connection);
      httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")));

      return httpContext;
   }

   private sealed class TestHttpContextAccessor(HttpContext? httpContext) : IHttpContextAccessor
   {
      public HttpContext? HttpContext { get; set; } = httpContext;
   }
}
