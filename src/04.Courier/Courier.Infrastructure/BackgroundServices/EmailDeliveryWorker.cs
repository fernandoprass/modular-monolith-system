using Courier.Application.Contracts;
using Courier.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IEmailOutboxService>();
            var processed = false;

            for (var i = 0; i < CourierConst.Worker.EmailDeliveryBatchSize; i++)
            {
               if (!await service.ProcessNextPendingAsync(stoppingToken))
               {
                  break;
               }

               processed = true;
            }

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
         catch (Exception ex)
         {
            _logger.LogError(ex, "Courier email delivery worker failed");
            await LogSystemErrorAsync(ex, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(CourierConst.Worker.EmailDeliveryErrorDelaySeconds), stoppingToken);
         }
      }

      _logger.LogInformation("Courier email delivery worker stopped");
   }

   private async Task LogSystemErrorAsync(Exception exception, CancellationToken cancellationToken)
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
}
