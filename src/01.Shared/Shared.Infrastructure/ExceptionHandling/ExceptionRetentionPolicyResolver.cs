using Microsoft.EntityFrameworkCore;
using Shared.Domain.Enums;
using System.Net;

namespace Shared.Infrastructure.ExceptionHandling;

public static class ExceptionRetentionPolicyResolver
{
   public static RetentionPolicy Resolve(Exception exception, int statusCode)
   {
      return exception switch
      {
         UnauthorizedAccessException => RetentionPolicy.Extended,
         DbUpdateException => RetentionPolicy.Standard,
         NotImplementedException => RetentionPolicy.Standard,
         TimeoutException => RetentionPolicy.Operational,
         TaskCanceledException => RetentionPolicy.Operational,
         OperationCanceledException => RetentionPolicy.Operational,
         KeyNotFoundException => RetentionPolicy.Operational,
         ArgumentException => RetentionPolicy.Operational,
         _ when statusCode == (int)HttpStatusCode.Unauthorized => RetentionPolicy.Extended,
         _ => RetentionPolicy.Operational
      };
   }
}
