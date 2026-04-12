using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Repositories;

internal class ParameterOverrideRepository(SharedDbContext dbContext) 
   : BaseRepository<ParameterOverride>(dbContext), IParameterOverrideRepository
{
   public async Task<ParameterOverride?> GetByParameterIdAndOwnerIdAsync(Guid parameterId, Guid ownerId, CancellationToken cancellationToken = default)
   {
      return await _dbSet.FirstOrDefaultAsync(pc => pc.ParameterId == parameterId && 
                                                    pc.OwnerId == ownerId, cancellationToken);
   }
}
