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
         _ when statusCode == (int)HttpStatusCode.Unauthorized => RetentionPolicy.LongTerm,
         _ => RetentionPolicy.Operational
      };
   }
}
