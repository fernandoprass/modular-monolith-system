using Microsoft.AspNetCore.Http;
using Shared.Domain.DTOs.Responses;
using System.Text.Json;

namespace Shared.Infrastructure.ExceptionHandling;

public static class ExceptionResponseWriter
{
   private static readonly JsonSerializerOptions JsonOptions = new()
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
   };

   public static async Task<ExceptionResponseDto> WriteAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken = default)
   {
      var exceptionResponse = ExceptionResponseFactory.Create(exception);

      httpContext.Response.ContentType = "application/json; charset=utf-8";
      httpContext.Response.StatusCode = exceptionResponse.StatusCode;

      var response = new
      {
         exceptionResponse.Message,
         exceptionResponse.Details
      };

      var bytes = JsonSerializer.SerializeToUtf8Bytes(response, JsonOptions);

      await httpContext.Response.Body.WriteAsync(bytes, cancellationToken);

      return exceptionResponse;
   }
}
