using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Infrastructure;
using Shared.Infrastructure.Security;

namespace Shared.API.Configure;

public static class SharedModule
{
   public static IServiceCollection AddSharedModule(this IServiceCollection services, IConfiguration configuration)
   {
      var connection = configuration.GetConnectionString(SharedConst.Database.ConnectionString)
         ?? throw new InvalidOperationException("Shared database connection string is required.");
      services.AddSharedInfrastructure(configuration, connection);
      services.AddHttpContextAccessor();
      services.AddScoped<IUserContext, AspNetUserContext>();
      return services;
   }
}
