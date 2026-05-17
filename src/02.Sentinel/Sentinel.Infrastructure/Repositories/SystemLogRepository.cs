using MongoDB.Driver;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;

namespace Sentinel.Infrastructure.Repositories;

public class SystemLogRepository(SentinelDbContext dbContext) : ISystemLogRepository
{
   public async Task AddAsync(SystemLog log, CancellationToken cancellationToken = default)
   {
      await dbContext.SystemLogs.InsertOneAsync(log, cancellationToken: cancellationToken);
   }

   public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await dbContext.SystemLogs
         .Find(log => log.Id == id)
         .AnyAsync(cancellationToken);
   }
}
