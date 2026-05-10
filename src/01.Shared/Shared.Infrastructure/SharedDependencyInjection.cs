using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Application.Validators;
using Shared.Domain;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.QueryRepositories;
using Shared.Infrastructure.Repositories;
using Shared.Infrastructure.UoW;

namespace Shared.Infrastructure;

public static class SharedDependencyInjection
{
   public static IServiceCollection AddSharedInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration,
       string connectionString)
   {
      ConfigureDbContext(services, configuration, connectionString);

      services.AddScoped<ISharedUnitOfWork, SharedUnitOfWork>();

      services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));
      services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

      services.AddScoped<IParameterRepository, ParameterRepository>();
      services.AddScoped<IParameterOverrideRepository, ParameterOverrideRepository>();
      services.AddScoped<IParameterQueryRepository, ParameterQueryRepository>();

      services.AddScoped<IParameterService, ParameterService>();
      services.AddScoped<IParameterValidator, ParameterValidator>();

      return services;
   }

   private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, string connectionString)
   {
      connectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : configuration.GetConnectionString(SharedConst.Database.ConnectionString);
      services.AddDbContext<SharedDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
   }
}