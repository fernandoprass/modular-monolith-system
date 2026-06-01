using Microsoft.AspNetCore.Diagnostics;
using Sentinel.Domain;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Shared.Application.Contracts;
using Shared.Infrastructure.ExceptionHandling;
using System.Text.Json;

namespace Sentinel.API.Middlewares;

public class GlobalExceptionHandler(
   ILogger<GlobalExceptionHandler> logger,
   IServiceProvider serviceProvider) : IExceptionHandler
{
   private readonly ILogger<GlobalExceptionHandler> _logger = logger;
   private readonly IServiceProvider _serviceProvider = serviceProvider;

   public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
   {
      _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

      httpContext.Response.ContentType = "application/json";
      var exceptionResponse = ExceptionResponseFactory.Create(exception);

      await SaveSystemLogAsync(httpContext, exception, exceptionResponse.StatusCode, cancellationToken);

      httpContext.Response.StatusCode = exceptionResponse.StatusCode;
      var response = new
      {
         exceptionResponse.Message,
         exceptionResponse.Details
      };

      await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
      return true;
   }

   private async Task SaveSystemLogAsync(
      HttpContext httpContext,
      Exception exception,
      int statusCode,
      CancellationToken cancellationToken)
   {
      try
      {
         using var scope = _serviceProvider.CreateScope();
         var unitOfWork = scope.ServiceProvider.GetRequiredService<ISentinelUnitOfWork>();
         var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();

         var systemLogEvent = SystemLogEventFactory.Create(SentinelConst.System.ModuleName, httpContext, exception, statusCode, userContext);
         var propertiesJson = JsonSerializer.Serialize(systemLogEvent.Properties);

         var systemLog = SystemLog.Create(
            systemLogEvent.Id,
            systemLogEvent.Level,
            systemLogEvent.Status,
            systemLogEvent.RetentionPolicy,
            systemLogEvent.Source,
            systemLogEvent.Message,
            systemLogEvent.Exception,
            systemLogEvent.StackTrace,
            systemLogEvent.RequestId,
            systemLogEvent.UserId,
            systemLogEvent.OrganizationId,
            propertiesJson);

         await unitOfWork.SystemLogs.AddAsync(systemLog, cancellationToken);
         await unitOfWork.SaveChangesAsync(cancellationToken);
      }
      catch (Exception saveException)
      {
         _logger.LogError(saveException, "Failed to persist Sentinel exception log");
      }
   }
}
