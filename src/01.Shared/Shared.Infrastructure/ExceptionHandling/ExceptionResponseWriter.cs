using Microsoft.AspNetCore.Http;
using Shared.Domain.DTOs.Responses;
using System.Text;
using System.Text.Json;

namespace Shared.Infrastructure.ExceptionHandling;

public static class ExceptionResponseWriter
{
   public static async Task<ExceptionResponseDto> WriteAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken = default)
   {
      var exceptionResponse = ExceptionResponseFactory.Create(exception);

      httpContext.Response.ContentType = "application/json";
      httpContext.Response.StatusCode = exceptionResponse.StatusCode;

      var response = new
      {
         exceptionResponse.Message,
         exceptionResponse.Details
      };

      string json = JsonSerializer.Serialize(response, options: new JsonSerializerOptions
      {
         PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });
      var bytes = Encoding.UTF8.GetBytes(json);

      await httpContext.Response.Body.WriteAsync(bytes, cancellationToken);

      return exceptionResponse;
   }
}
