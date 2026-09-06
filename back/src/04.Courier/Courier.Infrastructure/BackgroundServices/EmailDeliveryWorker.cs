using Courier.Application.Contracts;
using Courier.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Shared.Domain.Enums;

namespace Courier.Infrastructure.BackgroundServices;

public class EmailDeliveryWorker(
   IServiceProvider serviceProvider,
   ILogger<EmailDeliveryWorker> logger) : BackgroundService
{
   private readonly IServiceProvider _serviceProvider = serviceProvider;
   private readonly ILogger<EmailDeliveryWorker> _logger = logger;

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      _logger.LogInformation("Courier email delivery worker started");

      while (!stoppingToken.IsCancellationRequested)
      {
         try
         {
            var processed = await ProcessBatchAsync(stoppingToken);

            if (processed)
            {
               continue;
            }

            await Task.Delay(TimeSpan.FromSeconds(CourierConst.Worker.EmailDeliveryBatchDelaySeconds), stoppingToken);
         }
         catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
         {
            break;
         }
         catch (Exception ex) when (IsRetryableMongoException(ex))
         {
            _logger.LogWarning(
               "Courier MongoDB is unavailable for email delivery worker; retrying. {ErrorMessage}",
               ex.Message);
            await DelayAsync(CourierConst.Worker.EmailDeliveryErrorDelaySeconds, stoppingToken);
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Courier email delivery worker failed");
            await TryLogSystemErrorAsync(ex, stoppingToken);
            await DelayAsync(CourierConst.Worker.EmailDeliveryErrorDelaySeconds, stoppingToken);
         }
      }

      _logger.LogInformation("Courier email delivery worker stopped");
   }

   internal async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
   {
      using var scope = _serviceProvider.CreateScope();
      var service = scope.ServiceProvider.GetRequiredService<IEmailOutboxService>();
      var processed = false;

      for (var i = 0; i < CourierConst.Worker.EmailDeliveryBatchSize; i++)
      {
         if (!await service.ProcessNextPendingAsync(cancellationToken))
         {
            break;
         }

         processed = true;
      }

      return processed;
   }

   internal async Task TryLogSystemErrorAsync(Exception exception, CancellationToken cancellationToken)
   {
      try
      {
         using var scope = _serviceProvider.CreateScope();
         var logger = scope.ServiceProvider.GetRequiredService<ICourierLogger>();
         await logger.LogSystemAsync(
            SystemLogLevel.Error,
            SystemLogStatus.Failure,
            "Courier email delivery worker failed",
            exception,
            cancellationToken: cancellationToken);
      }
      catch (Exception logException)
      {
         _logger.LogError(logException, "Failed to log Courier email delivery worker error");
      }
   }

   private static bool IsRetryableMongoException(Exception exception)
   {
      return exception is TimeoutException
         || exception is MongoConnectionException
         || exception is MongoExecutionTimeoutException
         || (exception.InnerException != null && IsRetryableMongoException(exception.InnerException));
   }

   private static async Task DelayAsync(int seconds, CancellationToken cancellationToken)
   {
      try
      {
         await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
      }
   }
}
