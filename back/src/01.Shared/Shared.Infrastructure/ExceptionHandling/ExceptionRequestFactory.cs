using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.ExceptionHandling;

public static class ExceptionRequestFactory
{
   public static Dictionary<string, object> Create(HttpRequest httpRequest)
   {
      var properties = new Dictionary<string, object>
      {
         ["method"] = httpRequest.Method,
         ["scheme"] = httpRequest.Scheme,
         ["host"] = httpRequest.Host.ToString(),
         ["path"] = httpRequest.Path.ToString()
      };

      if (httpRequest.Query.Count > 0)
      {
         properties.Add("queryKeys", httpRequest.Query.Keys.ToArray());
      }

      if (!string.IsNullOrWhiteSpace(httpRequest.ContentType))
      {
         properties.Add("contentType", httpRequest.ContentType);
      }

      if (httpRequest.ContentLength.HasValue)
      {
         properties.Add("contentLength", httpRequest.ContentLength.Value);
      }

      return properties;
   }

   public static Dictionary<string, object> Create(HttpRequest httpRequest, int statusCode)
   {
      var properties = Create(httpRequest);
      properties.Add("statusCode", statusCode);

      return properties;
   }
}
