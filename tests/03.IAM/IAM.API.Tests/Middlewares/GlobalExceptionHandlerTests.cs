using IAM.API.Middlewares;
using IAM.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Interfaces;
using System.Text.Json;

namespace IAM.API.Tests.Middlewares;

public class GlobalExceptionHandlerTests
{
   [Fact]
   public async Task TryHandleAsync_ShouldReturnTrue()
   {
      var systemLogPublisher = Substitute.For<IExceptionSystemLogPublisher>();
      var handler = CreateHandler(systemLogPublisher);
      var httpContext = CreateHttpContext();
      var exception = new InvalidOperationException("Boom");

      var handled = await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

      Assert.True(handled);
      Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
   }

   [Fact]
   public async Task TryHandleAsync_ShouldPublishIamSystemLog()
   {
      var systemLogPublisher = Substitute.For<IExceptionSystemLogPublisher>();
      var handler = CreateHandler(systemLogPublisher);
      var httpContext = CreateHttpContext();
      var exception = new InvalidOperationException("Boom");

      await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

      await systemLogPublisher.Received(1).PublishAsync(
         IamConst.System.ModuleName,
         httpContext,
         exception,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task TryHandleAsync_ShouldNotThrow_WhenSystemLogPublishFails()
   {
      var systemLogPublisher = Substitute.For<IExceptionSystemLogPublisher>();
      systemLogPublisher
         .PublishAsync(
            Arg.Any<string>(),
            Arg.Any<HttpContext>(),
            Arg.Any<Exception>(),
            Arg.Any<CancellationToken>())
         .Returns(_ => throw new InvalidOperationException("Publisher failed"));

      var handler = CreateHandler(systemLogPublisher);
      var httpContext = CreateHttpContext();
      var exception = new InvalidOperationException("Boom");

      var handled = await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

      Assert.True(handled);
      Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
   }

   [Fact]
   public async Task TryHandleAsync_ShouldWriteErrorResponse()
   {
      var systemLogPublisher = Substitute.For<IExceptionSystemLogPublisher>();
      var handler = CreateHandler(systemLogPublisher);
      var httpContext = CreateHttpContext();
      var exception = new UnauthorizedAccessException("Denied");

      await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

      Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
      Assert.Equal("application/json; charset=utf-8", httpContext.Response.ContentType);

      httpContext.Response.Body.Position = 0;
      using var json = await JsonDocument.ParseAsync(httpContext.Response.Body, cancellationToken: TestContext.Current.CancellationToken);
      Assert.Equal("Unauthorized access.", json.RootElement.GetProperty("message").GetString());
   }

   private static GlobalExceptionHandler CreateHandler(
      IExceptionSystemLogPublisher systemLogPublisher,
      ILogger<GlobalExceptionHandler>? logger = null)
   {
      var services = new ServiceCollection();
      services.AddScoped(_ => systemLogPublisher);

      return new GlobalExceptionHandler(
         logger ?? Substitute.For<ILogger<GlobalExceptionHandler>>(),
         services.BuildServiceProvider());
   }

   private static HttpContext CreateHttpContext()
   {
      var httpContext = new DefaultHttpContext();
      httpContext.TraceIdentifier = "request-1";
      httpContext.Request.Method = HttpMethods.Get;
      httpContext.Request.Path = "/api/test";
      httpContext.Response.Body = new MemoryStream();

      return httpContext;
   }
}
