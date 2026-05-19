using Microsoft.AspNetCore.Http;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using System.Net;

namespace Shared.Infrastructure.ExceptionHandling;

public static class SystemLogEventFactory
{
   public static SystemLogEvent Create(
      string source,
      HttpContext httpContext,
      Exception exception,
      int statusCode,
      IUserContext userContext)
   {
      return Create(
         source,
         exception,
         statusCode,
         httpContext.TraceIdentifier,
         httpContext.Request.Path.ToString(),
         userContext);
   }

   public static SystemLogEvent Create(
      string source,
      Exception exception,
      int statusCode,
      string? requestId,
      string? path,
      IUserContext userContext)
   {
      return new SystemLogEvent
      {
         Id = Guid.CreateVersion7(),
         Timestamp = DateTime.UtcNow,
         Level = SystemLogLevel.Error,
         Status = GetStatus(statusCode),
         Source = source,
         Message = exception.Message,
         Exception = exception.GetType().Name,
         StackTrace = exception.StackTrace,
         RequestId = requestId,
         UserId = GetOptionalGuid(userContext.UserId),
         OrganizationId = GetOptionalGuid(userContext.UserOwnerId),
         Properties = new Dictionary<string, object>
         {
            ["path"] = path ?? string.Empty,
            ["statusCode"] = statusCode
         }
      };
   }

   private static SystemLogStatus GetStatus(int statusCode)
   {
      return statusCode switch
      {
         (int)HttpStatusCode.Unauthorized => SystemLogStatus.Unauthorized,
         _ => SystemLogStatus.Failure
      };
   }

   private static Guid? GetOptionalGuid(Guid value)
   {
      return value == Guid.Empty ? null : value;
   }
}
