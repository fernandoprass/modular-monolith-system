using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Application.Validators;
using Shared.Domain;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Authorization;
using Shared.Infrastructure.ExceptionHandling;
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
      RegisterParameterValueCache(services, configuration);
      services.AddScoped<IExceptionSystemLogPublisher, ExceptionSystemLogPublisher>();
      services.AddSharedAuthorization(configuration);

      return services;
   }

   public static IServiceCollection AddSharedAuthorization(
      this IServiceCollection services,
      IConfiguration configuration)
   {
      ConfigureDistributedCache(services, configuration);

      services.AddSingleton<DistributedRolePermissionCache>();
      services.AddSingleton<IRolePermissionCache>(provider => provider.GetRequiredService<DistributedRolePermissionCache>());
      services.AddSingleton<IRolePermissionCacheInvalidator>(provider => provider.GetRequiredService<DistributedRolePermissionCache>());
      services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

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

   private static void RegisterParameterValueCache(IServiceCollection services, IConfiguration configuration)
   {
      var redisConnectionString = configuration.GetConnectionString(SharedConst.Redis.ConnectionString);

      if (string.IsNullOrWhiteSpace(redisConnectionString))
      {
         services.AddScoped<IParameterCacheRespository, ParameterNullCacheRepository>();
         return;
      }

      services.AddScoped<IParameterCacheRespository, ParameterRedisCacheRepository>();
   }

   private static void ConfigureDistributedCache(IServiceCollection services, IConfiguration configuration)
   {
      var redisConnectionString = configuration.GetConnectionString(SharedConst.Redis.ConnectionString);

      if (string.IsNullOrWhiteSpace(redisConnectionString))
      {
         throw new InvalidOperationException("Redis connection string is required for distributed cache.");
      }

      services.AddStackExchangeRedisCache(options =>
      {
         options.Configuration = redisConnectionString;
      });
   }
}
