using Courier.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Courier.Infrastructure;

public static class CourierDependencyInjection
{
   public static IServiceCollection AddCourierInfrastructure(this IServiceCollection services, IConfiguration configuration)
   {
      ConfigureDbContext(configuration);
      ConfigureRedis(services, configuration);

      services.AddSingleton<CourierDbContext>();

      return services;
   }

   private static void ConfigureDbContext(IConfiguration configuration)
   {
      var connectionString = configuration.GetConnectionString(CourierConst.Database.ConnectionString);

      if (string.IsNullOrWhiteSpace(connectionString))
      {
         throw new InvalidOperationException("Courier MongoDB connection string is required.");
      }
   }

   private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
   {
      var redisConnectionString = configuration.GetConnectionString("Redis");

      if (string.IsNullOrWhiteSpace(redisConnectionString))
      {
         throw new InvalidOperationException("Redis connection string is required for Courier.");
      }

      services.AddSingleton<IConnectionMultiplexer>(_ =>
      {
         var options = ConfigurationOptions.Parse(redisConnectionString);
         return ConnectionMultiplexer.Connect(options);
      });
   }
}
