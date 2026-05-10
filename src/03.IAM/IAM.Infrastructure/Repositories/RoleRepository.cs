using IAM.Domain.Entities;
using IAM.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;

namespace IAM.Infrastructure.Repositories;

public class RoleRepository(IamDbContext dbContext) : BaseRepository<Role>(dbContext), IRoleRepository
{
   public override async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbSet
         .Include(r => r.RolePermissions)
         .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
   }
   public async Task<IEnumerable<Role?>> GetAllByOrganizationAsync(Guid? organizationId, CancellationToken cancellationToken = default)
   {
      return await _dbSet
         .Where(r => r.OrganizationId == organizationId)
         .ToListAsync(cancellationToken);
   }

}
