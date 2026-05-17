using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using MongoDB.Driver;

namespace Sentinel.Infrastructure.Repositories;

public class AuditLogRepository(SentinelDbContext dbContext) : IAuditLogRepository
{
   public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
   {
      await dbContext.AuditLogs.InsertOneAsync(log, cancellationToken: cancellationToken);
   }

   public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await dbContext.AuditLogs
         .Find(log => log.Id == id)
         .AnyAsync(cancellationToken);
   }
}
