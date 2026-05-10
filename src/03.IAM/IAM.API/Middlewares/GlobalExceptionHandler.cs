using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IAM.API.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
   private readonly ILogger<GlobalExceptionHandler> _logger = logger;

   public async ValueTask<bool> TryHandleAsync(
       HttpContext httpContext,
       Exception exception,
       CancellationToken cancellationToken)
   {
      _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

      httpContext.Response.ContentType = "application/json";
      // Default response for unexpected errors
      var statusCode = (int)HttpStatusCode.InternalServerError;
      var message = "An unexpected error occurred.";
      var details = exception.Message; // Hide in production
                                       // Specific handling for Database Exceptions
      if (exception is DbUpdateException dbUpdateException)
      {
         statusCode = (int)HttpStatusCode.BadRequest; // Or 409 Conflict
         message = "A database error occurred. This could be a constraint violation or invalid data.";
         details = dbUpdateException.InnerException?.Message ?? dbUpdateException.Message;
      }
      else if (exception is UnauthorizedAccessException)
      {
         statusCode = (int)HttpStatusCode.Unauthorized;
         message = "Unauthorized access.";
      }
      // You can add more specific exception checks here (e.g. ValidationException)
      httpContext.Response.StatusCode = statusCode;
      var response = new
      {
         Message = message,
         Details = details
      };
      await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
      // Return true to indicate the exception has been handled and shouldn't propagate further
      return true;
   }
}
