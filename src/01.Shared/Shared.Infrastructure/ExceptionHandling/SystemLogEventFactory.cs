using Microsoft.AspNetCore.Http;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using System.Net;

namespace Shared.Infrastructure.ExceptionHandling;

public static class SystemLogEventFactory
{
   public static SystemLogEvent Create(
      string module,
      HttpContext httpContext,
      Exception exception,
      IUserContext userContext)
   {
      var retentionPolicy = ExceptionRetentionPolicyResolver.Resolve(exception, httpContext.Response.StatusCode);
      var properties = ExceptionRequestFactory.Create(httpContext.Request, httpContext.Response.StatusCode);

      return new SystemLogEvent
      {
         Id = Guid.CreateVersion7(),
         Level = SystemLogLevel.Error,
         Status = GetStatus(httpContext.Response.StatusCode),
         RetentionPolicy = retentionPolicy,
         Module = module,
         Message = exception.Message,
         Exception = exception.GetType().Name,
         StackTrace = exception.StackTrace,
         RequestId = httpContext.TraceIdentifier,
         UserId = GetOptionalGuid(userContext.UserId),
         OrganizationId = GetOptionalGuid(userContext.UserOwnerId),
         Properties = properties
      };
   }

   private static SystemLogStatus GetStatus(int statusCode)
   {
      return statusCode == (int)HttpStatusCode.Unauthorized 
         ? SystemLogStatus.Unauthorized 
         : SystemLogStatus.Failure;
   }

   private static Guid? GetOptionalGuid(Guid value)
   {
      return value == Guid.Empty ? null : value;
   }
}
