using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

internal static class TestHttpContextFactory
{
   public static HttpContext Create(
      int statusCode = 500,
      string requestId = "request-1",
      string[]? queryKeys = null,
      string? contentType = null,
      long? contentLength = null)
   {
      var httpContext = Substitute.For<HttpContext>();
      var request = Substitute.For<HttpRequest>();
      var response = Substitute.For<HttpResponse>();

      request.Method.Returns(HttpMethods.Get);
      request.Path.Returns(new PathString("/api/test"));
      request.Scheme.Returns("https");
      request.Host.Returns(new HostString("localhost"));
      request.QueryString.Returns(queryKeys is { Length: > 0 } ? new QueryString("?ignored=1") : QueryString.Empty);
      request.ContentType.Returns(contentType);
      request.ContentLength.Returns(contentLength);

      var query = Substitute.For<IQueryCollection>();
      query.Count.Returns(queryKeys?.Length ?? 0);
      query.Keys.Returns(queryKeys ?? []);
      request.Query.Returns(query);

      response.StatusCode.Returns(statusCode);
      response.Body.Returns(new MemoryStream());

      httpContext.Request.Returns(request);
      httpContext.Response.Returns(response);
      httpContext.TraceIdentifier.Returns(requestId);

      return httpContext;
   }
}
