using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Application.Validators;
using Shared.Domain;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.QueryRepositories;
using Shared.Infrastructure.Repositories;
using Shared.Infrastructure.UoW;
using StackExchange.Redis;

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

      ConfigureRedis(services, configuration);

      return services;
   }

   private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, string connectionString)
   {
      var resolvedConnectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : configuration.GetConnectionString(SharedConst.Database.ConnectionString);

      if (string.IsNullOrWhiteSpace(resolvedConnectionString))
      {
         throw new InvalidOperationException("Shared database connection string is required.");
      }

      services.AddDbContext<SharedDbContext>(options => options.UseNpgsql(resolvedConnectionString).UseSnakeCaseNamingConvention());
   }

   private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
   {
      var redisConnectionString = configuration.GetConnectionString(SharedConst.Redis.ConnectionString);

      if (string.IsNullOrWhiteSpace(redisConnectionString))
      {
         return;
      }

      services.AddSingleton<IConnectionMultiplexer>(_ =>
      {
         var options = ConfigurationOptions.Parse(redisConnectionString);
         return ConnectionMultiplexer.Connect(options);
      });

      services.AddScoped<IEventPublisher, RedisEventPublisher>();
   }
}
