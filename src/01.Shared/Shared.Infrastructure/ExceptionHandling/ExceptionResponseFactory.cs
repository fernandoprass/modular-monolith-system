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

      if (exception is DbUpdateException dbUpdateException)
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
