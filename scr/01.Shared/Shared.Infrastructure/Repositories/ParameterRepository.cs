using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Repositories;

internal class ParameterRepository(SharedDbContext dbContext) : BaseRepository<Parameter>(dbContext), IParameterRepository
{
   public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
   {
      return await _dbSet.AnyAsync(e => e.Key.Equals(key), cancellationToken);
   }

   public async Task<Parameter?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
   {
      return await _dbSet.FirstOrDefaultAsync(p => p.Key == key, cancellationToken);
   }
}
