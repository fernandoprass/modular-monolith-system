using Microsoft.Extensions.Hosting;

namespace Courier.Infrastructure.BackgroundServices;

public class CourierIndexInitializer(CourierDbContext dbContext) : IHostedService
{
   private readonly CourierDbContext _dbContext = dbContext;

   public async Task StartAsync(CancellationToken cancellationToken)
   {
      await _dbContext.ConfigureIndexesAsync(cancellationToken);
   }

   public Task StopAsync(CancellationToken cancellationToken)
   {
      return Task.CompletedTask;
   }
}
