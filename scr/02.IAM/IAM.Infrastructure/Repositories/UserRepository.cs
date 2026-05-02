using IAM.Domain.Entities;
using IAM.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;

namespace IAM.Infrastructure.Repositories;

public class UserRepository(IamDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
{
   public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await dbContext.Users
          .Include(u => u.UserRoles)
          .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
   }

   public async Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
   {
      return await dbContext.Users
                    .Where(u => u.OrganizationId == organizationId)
                    .ToListAsync(cancellationToken);
   }
}