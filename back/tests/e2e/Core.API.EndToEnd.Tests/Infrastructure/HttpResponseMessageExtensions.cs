using System.Net.Http.Json;
using System.Text.Json;

namespace Core.API.EndToEnd.Tests.Infrastructure;

internal static class HttpResponseMessageExtensions
{
   private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

   public static async Task EnsureSuccessStatusCodeAsync(
      this HttpResponseMessage response,
      CancellationToken cancellationToken)
   {
      if (response.IsSuccessStatusCode)
      {
         return;
      }

      var body = await response.Content.ReadAsStringAsync(cancellationToken);
      throw new InvalidOperationException($"Expected success response but got {(int)response.StatusCode}. Body: {body}");
   }

   public static async Task<T> ReadResultDataAsync<T>(
      this HttpResponseMessage response,
      CancellationToken cancellationToken) where T : class
   {
      var result = await response.Content.ReadFromJsonAsync<ApiResult<T>>(JsonOptions, cancellationToken);

      if (result?.Data is null)
      {
         var body = await response.Content.ReadAsStringAsync(cancellationToken);
         throw new InvalidOperationException($"Response did not contain data. Body: {body}");
      }

      return result.Data;
   }
}
