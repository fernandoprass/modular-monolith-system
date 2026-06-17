using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Contracts;
using Sentinel.Application.Services;
using Sentinel.Domain.Interfaces;
using Sentinel.Domain.QueryRepositories;
using Sentinel.Infrastructure.BackgroundServices;
using Sentinel.Infrastructure.QueryRepositories;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Infrastructure.UoW;
using StackExchange.Redis;

namespace Sentinel.Infrastructure;

public static class SentinelDependencyInjection
{
   public static IServiceCollection AddSentinelInfrastructure(this IServiceCollection services, IConfiguration configuration)
   {
      ConfigureDbContext(services, configuration);
      ConfigureRedis(services, configuration);

      services.AddSingleton<SentinelDbContext>();
      services.AddScoped<ISentinelUnitOfWork, SentinelUnitOfWork>();
      services.AddScoped<IAuditLogRepository, AuditLogRepository>();
      services.AddScoped<ISystemLogRepository, SystemLogRepository>();
      services.AddScoped<ISentinelLogQueryRepository, SentinelLogQueryRepository>();
      services.AddScoped<ISentinelLogService, SentinelLogService>();

      if (IsHostedServicesEnabled(configuration))
      {
         services.AddHostedService<AuditLogConsumer>();
         services.AddHostedService<SystemLogConsumer>();
      }

      return services;
   }

   private static bool IsHostedServicesEnabled(IConfiguration configuration)
   {
      var configuredValue = configuration["Sentinel:HostedServicesEnabled"];

      return !bool.TryParse(configuredValue, out var enabled) || enabled;
   }

   private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
   {
      var connectionString = configuration.GetConnectionString("SentinelDb");

      if (string.IsNullOrWhiteSpace(connectionString))
      {
         throw new InvalidOperationException("Sentinel MongoDB connection string is required.");
      }
   }

   private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
   {
      var redisConnectionString = configuration.GetConnectionString("Redis");

      if (string.IsNullOrWhiteSpace(redisConnectionString))
      {
         throw new InvalidOperationException("Redis connection string is required for Sentinel consumers.");
      }

      services.AddSingleton<IConnectionMultiplexer>(_ =>
      {
         var options = ConfigurationOptions.Parse(redisConnectionString);
         return ConnectionMultiplexer.Connect(options);
      });
   }
}
