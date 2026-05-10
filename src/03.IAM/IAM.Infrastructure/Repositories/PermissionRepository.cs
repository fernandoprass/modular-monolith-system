using IAM.Domain.Entities;
using IAM.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;

namespace IAM.Infrastructure.Repositories;

public class PermissionRepository(IamDbContext dbContext) : BaseRepository<Permission>(dbContext), IPermissionRepository
{
   public async Task<int> CountByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
   {
      return await _dbSet.CountAsync(permission => ids.Contains(permission.Id), cancellationToken);
   }

   public async Task<IEnumerable<Permission>> GetAll(CancellationToken cancellationToken = default)
   {
      return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
   }
}
