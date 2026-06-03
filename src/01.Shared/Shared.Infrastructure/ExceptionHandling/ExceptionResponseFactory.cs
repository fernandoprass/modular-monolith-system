using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs.Responses;
using System.Net;

namespace Shared.Infrastructure.ExceptionHandling;

public static class ExceptionResponseFactory
{
   public static ExceptionResponseDto Create(Exception exception)
   {
      var response = new ExceptionResponseDto
      {
         StatusCode = (int)HttpStatusCode.InternalServerError,
         Message = "An unexpected error occurred.",
         Details = exception.Message
      };

      if (exception is OperationCanceledException)
      {
         response.StatusCode = (int)HttpStatusCode.RequestTimeout;
         response.Message = "The request was canceled.";
      }
      else if (exception is TimeoutException)
      {
         response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
         response.Message = "The request timed out.";
      }
      else if (exception is KeyNotFoundException)
      {
         response.StatusCode = (int)HttpStatusCode.NotFound;
         response.Message = "The requested resource was not found.";
      }
      else if (exception is ArgumentException)
      {
         response.StatusCode = (int)HttpStatusCode.BadRequest;
         response.Message = "The request is invalid.";
      }
      else if (exception is NotImplementedException)
      {
         response.StatusCode = (int)HttpStatusCode.NotImplemented;
         response.Message = "This feature is not implemented.";
      }
      else if (exception is DbUpdateException dbUpdateException)
      {
         response.StatusCode = (int)HttpStatusCode.BadRequest;
         response.Message = "A database error occurred. This could be a constraint violation or invalid data.";
         response.Details = dbUpdateException.InnerException?.Message ?? dbUpdateException.Message;
      }
      else if (exception is UnauthorizedAccessException)
      {
         response.StatusCode = (int)HttpStatusCode.Unauthorized;
         response.Message = "Unauthorized access.";
      }

      return response;
   }
}
