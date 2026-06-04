using Courier.Infrastructure;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Infrastructure;
using Shared.Infrastructure.Security;

namespace Courier.API.Configure;

public static class CourierModule
{
   public static IServiceCollection AddCourierModule(
      this IServiceCollection services,
      IConfiguration configuration)
   {
      services.AddCourierInfrastructure(configuration);

      var sharedConnectionString = configuration.GetConnectionString(SharedConst.Database.ConnectionString)
         ?? throw new InvalidOperationException("Shared database connection string is required.");

      services.AddSharedInfrastructure(configuration, sharedConnectionString);
      services.AddHttpContextAccessor();
      services.AddScoped<IUserContext, AspNetUserContext>();

      return services;
   }
}
