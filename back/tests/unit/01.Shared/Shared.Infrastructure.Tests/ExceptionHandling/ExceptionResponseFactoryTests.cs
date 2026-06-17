using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class ExceptionResponseFactoryTests
{
   [Theory]
   [InlineData("db", StatusCodes.Status400BadRequest, "A database error occurred. This could be a constraint violation or invalid data.")]
   [InlineData("unauthorized", StatusCodes.Status401Unauthorized, "Unauthorized access.")]
   [InlineData("canceled", StatusCodes.Status408RequestTimeout, "The request was canceled.")]
   [InlineData("timeout", StatusCodes.Status504GatewayTimeout, "The request timed out.")]
   [InlineData("notfound", StatusCodes.Status404NotFound, "The requested resource was not found.")]
   [InlineData("argument", StatusCodes.Status400BadRequest, "The request is invalid.")]
   [InlineData("argumentNull", StatusCodes.Status400BadRequest, "The request is invalid.")]
   [InlineData("taskCanceled", StatusCodes.Status408RequestTimeout, "The request was canceled.")]
   [InlineData("notimplemented", StatusCodes.Status501NotImplemented, "This feature is not implemented.")]
   [InlineData("generic", StatusCodes.Status500InternalServerError, "An unexpected error occurred.")]
   public void Create_ShouldReturnExpectedResponse(string exceptionType, int expectedStatusCode, string expectedMessage)
   {
      var exception = CreateException(exceptionType);

      var response = ExceptionResponseFactory.Create(exception);

      Assert.Equal(expectedStatusCode, response.StatusCode);
      Assert.Equal(expectedMessage, response.Message);
      Assert.False(string.IsNullOrWhiteSpace(response.Details));
   }

   private static Exception CreateException(string exceptionType)
   {
      return exceptionType switch
      {
         "db" => new DbUpdateException("Database failed", new InvalidOperationException("Unique constraint")),
         "unauthorized" => new UnauthorizedAccessException("Denied"),
         "canceled" => new OperationCanceledException("Canceled"),
         "timeout" => new TimeoutException("Slow"),
         "notfound" => new KeyNotFoundException("Missing"),
         "argument" => new ArgumentException("Bad"),
         "argumentNull" => new ArgumentNullException("name"),
         "taskCanceled" => new TaskCanceledException("Canceled"),
         "notimplemented" => new NotImplementedException("Later"),
         _ => new InvalidOperationException("Boom")
      };
   }
}
