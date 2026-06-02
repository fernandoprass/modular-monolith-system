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
      HttpRequest request,
      Exception exception,
      int statusCode,
      string? requestId,
      IUserContext userContext)
   {
      var retentionPolicy = ExceptionRetentionPolicyResolver.Resolve(exception, statusCode);
      var properties = ExceptionRequestFactory.Create(request, statusCode);

      return new SystemLogEvent
      {
         Id = Guid.CreateVersion7(),
         Level = SystemLogLevel.Error,
         Status = GetStatus(statusCode),
         RetentionPolicy = retentionPolicy,
         Source = source,
         Message = exception.Message,
         Exception = exception.GetType().Name,
         StackTrace = exception.StackTrace,
         RequestId = requestId,
         UserId = GetOptionalGuid(userContext.UserId),
         OrganizationId = GetOptionalGuid(userContext.UserOwnerId),
         Properties = properties
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
