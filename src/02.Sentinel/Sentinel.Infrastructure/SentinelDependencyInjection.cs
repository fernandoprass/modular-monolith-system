using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Domain;
using Sentinel.Domain.Interfaces;
using Sentinel.Application.Contracts;
using Sentinel.Application.Services;
using Sentinel.Domain.QueryRepositories;
using Sentinel.Infrastructure.Authorization;
using Sentinel.Infrastructure.BackgroundServices;
using Sentinel.Infrastructure.QueryRepositories;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Infrastructure.UoW;
using Shared.Application.Contracts;
using StackExchange.Redis;

namespace Sentinel.Infrastructure;

public static class SentinelDependencyInjection
{
   public static IServiceCollection AddSentinelInfrastructure(this IServiceCollection services, IConfiguration configuration)
   {
      ConfigureDbContext(services, configuration);
      ConfigureRedis(services, configuration);

      services.AddScoped<ISentinelUnitOfWork, SentinelUnitOfWork>();
      services.AddScoped<IAuditLogRepository, AuditLogRepository>();
      services.AddScoped<ISystemLogRepository, SystemLogRepository>();
      services.AddScoped<ISentinelLogQueryRepository, SentinelLogQueryRepository>();
      services.AddScoped<ISentinelLogService, SentinelLogService>();
      services.AddScoped<IRolePermissionProvider, SentinelRolePermissionProvider>();

      services.AddHostedService<AuditEventConsumer>();
      services.AddHostedService<SystemLogConsumer>();

      return services;
   }

   private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
   {
      var connectionString = configuration.GetConnectionString(SentinelConst.Database.ConnectionString);
      services.AddDbContext<SentinelDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
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
