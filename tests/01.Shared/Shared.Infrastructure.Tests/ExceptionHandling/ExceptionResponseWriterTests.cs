using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shared.Infrastructure.ExceptionHandling;
using System.Text.Json;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class ExceptionResponseWriterTests
{
   [Fact]
   public async Task WriteAsync_ShouldWriteJsonResponse()
   {
      var httpContext = Substitute.For<HttpContext>();
      var response = Substitute.For<HttpResponse>();
      var body = new MemoryStream();

      response.Body.Returns(body);
      httpContext.Response.Returns(response);

      var exceptionResponse = await ExceptionResponseWriter.WriteAsync(
         httpContext,
         new UnauthorizedAccessException("Denied"),
         TestContext.Current.CancellationToken);

      Assert.Equal(StatusCodes.Status401Unauthorized, exceptionResponse.StatusCode);
      Assert.Equal("application/json; charset=utf-8", response.ContentType);
      Assert.Equal(StatusCodes.Status401Unauthorized, response.StatusCode);

      body.Position = 0;
      using var json = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
      Assert.Equal("Unauthorized access.", json.RootElement.GetProperty("message").GetString());
      Assert.Equal("Denied", json.RootElement.GetProperty("details").GetString());
   }
}
