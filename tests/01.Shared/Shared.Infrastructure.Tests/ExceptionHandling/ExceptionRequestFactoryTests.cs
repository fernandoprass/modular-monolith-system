using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class ExceptionRequestFactoryTests
{
   [Fact]
   public void Create_ShouldReturnSafeRequestProperties()
   {
      var request = Substitute.For<HttpRequest>();
      request.Method.Returns(HttpMethods.Post);
      request.Scheme.Returns("https");
      request.Host.Returns(new HostString("localhost"));
      request.Path.Returns(new PathString("/api/test"));
      request.QueryString.Returns(new QueryString("?token=secret&page=1"));
      request.ContentType.Returns("application/json");
      request.ContentLength.Returns(42);

      var query = Substitute.For<IQueryCollection>();
      query.Count.Returns(2);
      query.Keys.Returns(["token", "page"]);
      request.Query.Returns(query);

      var properties = ExceptionRequestFactory.Create(request, StatusCodes.Status500InternalServerError);

      Assert.Equal(HttpMethods.Post, properties["method"]);
      Assert.Equal("https", properties["scheme"]);
      Assert.Equal("localhost", properties["host"]);
      Assert.Equal("/api/test", properties["path"]);
      Assert.Equal("application/json", properties["contentType"]);
      Assert.Equal(42L, properties["contentLength"]);
      Assert.Equal(StatusCodes.Status500InternalServerError, properties["statusCode"]);
      Assert.Equal(["token", "page"], Assert.IsType<string[]>(properties["queryKeys"]));
      Assert.False(properties.ContainsKey("queryString"));
   }

   [Fact]
   public void Create_ShouldSkipOptionalProperties_WhenRequestDoesNotContainThem()
   {
      var request = Substitute.For<HttpRequest>();
      request.Method.Returns(HttpMethods.Get);
      request.Scheme.Returns("https");
      request.Host.Returns(new HostString("localhost"));
      request.Path.Returns(new PathString("/api/test"));

      var query = Substitute.For<IQueryCollection>();
      query.Count.Returns(0);
      query.Keys.Returns([]);
      request.Query.Returns(query);

      var properties = ExceptionRequestFactory.Create(request);

      Assert.False(properties.ContainsKey("queryKeys"));
      Assert.False(properties.ContainsKey("contentType"));
      Assert.False(properties.ContainsKey("contentLength"));
   }
}
