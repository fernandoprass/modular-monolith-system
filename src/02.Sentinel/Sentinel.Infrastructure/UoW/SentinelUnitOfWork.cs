using Sentinel.Domain.Interfaces;
using Sentinel.Infrastructure.Repositories;

namespace Sentinel.Infrastructure.UoW;

public class SentinelUnitOfWork(SentinelDbContext dbContext) : ISentinelUnitOfWork
{
   public IAuditLogRepository AuditLogs => new AuditLogRepository(dbContext);
   public ISystemLogRepository SystemLogs => new SystemLogRepository(dbContext);

   public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
   {
      return dbContext.SaveChangesAsync(cancellationToken);
   }

   public void Dispose()
   {
      dbContext.Dispose();
   }
}
