using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class ExceptionResponseFactoryTests
{
   [Theory]
   [InlineData("db", StatusCodes.Status400BadRequest, "A database error occurred. This could be a constraint violation or invalid data.")]
   [InlineData("unauthorized", StatusCodes.Status401Unauthorized, "Unauthorized access.")]
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
         _ => new InvalidOperationException("Boom")
      };
   }
}
