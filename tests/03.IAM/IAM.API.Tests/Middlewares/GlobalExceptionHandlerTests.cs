using FluentAssertions;
using IAM.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System.Text.Json;

namespace IAM.API.Tests.Middlewares;

public class GlobalExceptionHandlerTests
{
   [Theory]
   [InlineData("db", StatusCodes.Status400BadRequest, "A database error occurred. This could be a constraint violation or invalid data.", RetentionPolicy.Standard)]
   [InlineData("unauthorized", StatusCodes.Status401Unauthorized, "Unauthorized access.", RetentionPolicy.Extended)]
   [InlineData("generic", StatusCodes.Status500InternalServerError, "An unexpected error occurred.", RetentionPolicy.Operational)]
   public async Task TryHandleAsync_ShouldReturnErrorResponseAndPublishSystemLog(
      string exceptionType,
      int expectedStatusCode,
      string expectedMessage,
      RetentionPolicy expectedRetentionPolicy)
   {
      var systemLogPublisher = Substitute.For<IExceptionSystemLogPublisher>();
      var services = new ServiceCollection();
      services.AddScoped(_ => systemLogPublisher);
      var serviceProvider = services.BuildServiceProvider();
      var handler = new GlobalExceptionHandler(Substitute.For<ILogger<GlobalExceptionHandler>>(), serviceProvider);
      var httpContext = new DefaultHttpContext();
      httpContext.Response.Body = new MemoryStream();
      var exception = CreateException(exceptionType);

      var handled = await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

      handled.Should().BeTrue();
      httpContext.Response.StatusCode.Should().Be(expectedStatusCode);

      httpContext.Response.Body.Position = 0;
      using var response = await JsonDocument.ParseAsync(httpContext.Response.Body, cancellationToken: TestContext.Current.CancellationToken);
      response.RootElement.GetProperty("message").GetString().Should().Be(expectedMessage);

      await systemLogPublisher.Received(1).PublishAsync(
         "IAM",
         exception,
         expectedStatusCode,
         httpContext.TraceIdentifier,
         httpContext.Request.Path.ToString(),
         expectedRetentionPolicy,
         Arg.Any<CancellationToken>());
   }

   private static Exception CreateException(string exceptionType)
   {
      return exceptionType switch
      {
         "db" => new DbUpdateException("Database failed", new InvalidOperationException("Unique constraint")),
         "unauthorized" => new UnauthorizedAccessException("Denied"),
         _ => new InvalidOperationException("Boom")
      };
   }
}
