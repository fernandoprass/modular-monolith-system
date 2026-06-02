using IAM.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.ExceptionHandling;

namespace IAM.API.Middlewares;

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

      await ExceptionResponseWriter.WriteAsync(httpContext, exception, cancellationToken);

      await PublishSystemLogAsync(httpContext, exception, cancellationToken);
      return true;
   }

   private async Task PublishSystemLogAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
   {
      try
      {
         using var scope = _serviceProvider.CreateScope();
         var publisher = scope.ServiceProvider.GetRequiredService<IExceptionSystemLogPublisher>();

         var properties = ExceptionRequestFactory.Create(httpContext.Request, httpContext.Response.StatusCode);

         await publisher.PublishAsync(
            IamConst.System.ModuleName,
            httpContext.Request,
            exception,
            httpContext.Response.StatusCode,
            httpContext.TraceIdentifier,
            cancellationToken);
      }
      catch (Exception publishException)
      {
         _logger.LogError(publishException, "Failed to publish IAM exception log");
      }
   }
}
