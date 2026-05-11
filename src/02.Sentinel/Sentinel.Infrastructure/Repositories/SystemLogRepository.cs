using Microsoft.EntityFrameworkCore;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Shared.Infrastructure.Repositories;

namespace Sentinel.Infrastructure.Repositories;

public class SystemLogRepository(SentinelDbContext dbContext) : BaseRepository<SystemLog>(dbContext), ISystemLogRepository
{
   public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await dbContext.SystemLogs.AsNoTracking().AnyAsync(s => s.Id == id, cancellationToken);
   }
}
