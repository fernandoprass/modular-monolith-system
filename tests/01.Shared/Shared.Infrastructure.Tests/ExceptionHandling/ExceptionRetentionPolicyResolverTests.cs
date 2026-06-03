using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Enums;
using Shared.Infrastructure.ExceptionHandling;

namespace Shared.Infrastructure.Tests.ExceptionHandling;

public class ExceptionRetentionPolicyResolverTests
{
   [Theory]
   [InlineData("unauthorized", StatusCodes.Status401Unauthorized, RetentionPolicy.Extended)]
   [InlineData("generic", StatusCodes.Status401Unauthorized, RetentionPolicy.Extended)]
   [InlineData("db", StatusCodes.Status400BadRequest, RetentionPolicy.Standard)]
   [InlineData("canceled", StatusCodes.Status408RequestTimeout, RetentionPolicy.Operational)]
   [InlineData("taskCanceled", StatusCodes.Status408RequestTimeout, RetentionPolicy.Operational)]
   [InlineData("timeout", StatusCodes.Status504GatewayTimeout, RetentionPolicy.Operational)]
   [InlineData("notfound", StatusCodes.Status404NotFound, RetentionPolicy.Operational)]
   [InlineData("argument", StatusCodes.Status400BadRequest, RetentionPolicy.Operational)]
   [InlineData("argumentNull", StatusCodes.Status400BadRequest, RetentionPolicy.Operational)]
   [InlineData("notimplemented", StatusCodes.Status501NotImplemented, RetentionPolicy.Standard)]
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
         "canceled" => new OperationCanceledException("Canceled"),
         "taskCanceled" => new TaskCanceledException("Canceled"),
         "timeout" => new TimeoutException("Slow"),
         "notfound" => new KeyNotFoundException("Missing"),
         "argument" => new ArgumentException("Bad"),
         "argumentNull" => new ArgumentNullException("name"),
         "notimplemented" => new NotImplementedException("Later"),
         _ => new InvalidOperationException("Boom")
      };
   }
}
