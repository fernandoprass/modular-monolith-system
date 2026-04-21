using IAM.Domain.Entities;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.QueryRepositories;

public class RoleQueryRepository(IamDbContext context) : IRoleQueryRepository
{
   private readonly IamDbContext _context = context;

   public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _context.Roles
         .AsNoTracking()
         .Include(r => r.RolePermissions)
            .ThenInclude(rf => rf.Permission)
         .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
   }

   public async Task<IEnumerable<Role>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken = default)
   {
      return await _context.Roles
         .AsNoTracking()
         .Where(r => r.OrganizationId == null || r.OrganizationId == organizationId)
         .Include(r => r.RolePermissions)
            .ThenInclude(rf => rf.Permission)
         .ToListAsync(cancellationToken);
   }

   public async Task<bool> NameExistsAsync(string name, Guid? organizationId, CancellationToken cancellationToken = default)
   {
      return await _context.Roles
         .AnyAsync(r => r.Name == name && r.OrganizationId == organizationId, cancellationToken);
   }

   public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      return await _context.UserRoles
          .AsNoTracking()
          .Where(ur => ur.UserId == userId)
          .SelectMany(ur => ur.Role.RolePermissions.Select(rf => rf.Permission))
          .Distinct()
          .ToListAsync(cancellationToken);
   }
}
