using Sentinel.Infrastructure;
using Shared.Application.Contracts;
using Shared.Infrastructure;
using Shared.Infrastructure.Security;

namespace Sentinel.API.Configure;

public static class SentinelModule
{
   public static IServiceCollection AddSentinelModule(
      this IServiceCollection services,
      IConfiguration configuration)
   {
      services.AddSentinelInfrastructure(configuration);
      services.AddSharedAuthorization(configuration);
      services.AddHttpContextAccessor();
      services.AddScoped<IUserContext, AspNetUserContext>();

      return services;
   }
}
