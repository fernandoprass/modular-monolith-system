using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Courier.Infrastructure.BackgroundServices;

public class CourierIndexInitializer(
   CourierDbContext dbContext,
   ILogger<CourierIndexInitializer> logger) : BackgroundService
{
   private readonly CourierDbContext _dbContext = dbContext;
   private readonly ILogger<CourierIndexInitializer> _logger = logger;

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      while (!stoppingToken.IsCancellationRequested)
      {
         try
         {
            await _dbContext.ConfigureIndexesAsync(stoppingToken);
            _logger.LogInformation("Courier MongoDB indexes configured");
            return;
         }
         catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
         {
            return;
         }
         catch (Exception ex) when (IsRetryableMongoException(ex))
         {
            _logger.LogWarning(
               "Courier MongoDB is unavailable while configuring indexes; retrying. {ErrorMessage}",
               ex.Message);
            await DelayAsync(stoppingToken);
         }
      }
   }

   private static bool IsRetryableMongoException(Exception exception)
   {
      return exception is TimeoutException
         || exception is MongoConnectionException
         || exception is MongoExecutionTimeoutException
         || (exception.InnerException != null && IsRetryableMongoException(exception.InnerException));
   }

   private static async Task DelayAsync(CancellationToken cancellationToken)
   {
      try
      {
         await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
      }
   }
}
