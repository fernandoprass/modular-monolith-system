using Microsoft.AspNetCore.Diagnostics;
using IAM.Domain;
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

      httpContext.Response.ContentType = "application/json";

      var exceptionResponse = ExceptionResponseFactory.Create(exception);

      using var scope = _serviceProvider.CreateScope();
      var systemLogPublisher = scope.ServiceProvider.GetRequiredService<IExceptionSystemLogPublisher>();

      await systemLogPublisher.PublishAsync(
         IamConst.System.ModuleName,
         exception,
         exceptionResponse.StatusCode,
         httpContext.TraceIdentifier,
         httpContext.Request.Path.ToString(),
         cancellationToken);

      httpContext.Response.StatusCode = exceptionResponse.StatusCode;
      var response = new
      {
         exceptionResponse.Message,
         exceptionResponse.Details
      };

      await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
      return true;
   }
}
