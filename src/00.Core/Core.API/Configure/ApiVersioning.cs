using Asp.Versioning;

namespace Core.API.Configure;

internal static class ApiVersioning
{
   public static void Configure(WebApplicationBuilder builder)
   {
      builder.Services.AddApiVersioning(options =>
      {
         options.DefaultApiVersion = new ApiVersion(1);
         options.ReportApiVersions = true;
         options.AssumeDefaultVersionWhenUnspecified = true;
         options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
      });
   }
}
