using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Enums;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class ExceptionRetentionPolicyResolverTests
{
   [Theory]
   [InlineData("unauthorized", StatusCodes.Status401Unauthorized, RetentionPolicy.Extended)]
   [InlineData("db", StatusCodes.Status400BadRequest, RetentionPolicy.Standard)]
   [InlineData("generic", StatusCodes.Status500InternalServerError, RetentionPolicy.Operational)]
   public void Resolve_ShouldReturnExpectedPolicy(
      string exceptionType,
      int statusCode,
      RetentionPolicy expectedPolicy)
   {
      var exception = CreateException(exceptionType);

      var retentionPolicy = ExceptionRetentionPolicyResolver.Resolve(exception, statusCode);

      Assert.Equal(expectedPolicy, retentionPolicy);
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
