using Microsoft.EntityFrameworkCore;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Shared.Infrastructure.Repositories;

namespace Sentinel.Infrastructure.Repositories;

public class AuditLogRepository(SentinelDbContext dbContext) : BaseRepository<AuditLog>(dbContext), IAuditLogRepository
{
   public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await dbContext.AuditLogs.AsNoTracking().AnyAsync(a => a.Id == id, cancellationToken);
   }
}
