using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.QueryRepositories;

public class RoleQueryRepository(IamDbContext dbContext) : IRoleQueryRepository
{
   private readonly IamDbContext _dbContext = dbContext;

   public async Task<int> CountRolesByRoleIdsAsync(
    IEnumerable<Guid> ids,
    Guid organizationId,
    bool isSystemAdminUser,
    CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithOrganizationContextFilter(organizationId, isSystemAdminUser);

      return await query.CountAsync(r => ids.Contains(r.Id) && r.IsActive, cancellationToken);
   }

   public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Roles
         .AsNoTracking()
         .Include(r => r.RolePermissions)
            .ThenInclude(rf => rf.Permission)
         .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
   }

   public async Task<IEnumerable<RoleDto>> GetByNameAsync(
       string? name,
       Guid organizationId,
       bool isSystemAdminUser,
       CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithOrganizationContextFilter(organizationId, isSystemAdminUser);

      if (!string.IsNullOrWhiteSpace(name))
      {
         query = query.Where(r => EF.Functions.ILike(r.Name, $"%{name}%"));
      }

      return await query
          .Select(r => r.ToRoleDto())
          .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<Permission>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.UserRoles
          .AsNoTracking()
          .Where(ur => ur.UserId == userId)
          .SelectMany(ur => ur.Role.RolePermissions.Select(rf => rf.Permission))
          .Distinct()
          .ToListAsync(cancellationToken);
   }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(
        Guid roleId, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.ToPermissionDto())
            .ToListAsync(cancellationToken);
    }

   public async Task<bool> NameExistsAsync(string name, Guid? organizationId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Roles
         .AnyAsync(r => r.Name == name && r.OrganizationId == organizationId, cancellationToken);
   }

   private IQueryable<Role> CreateQueryWithOrganizationContextFilter(Guid organizationId, bool isSystemAdminUser)
   {
      var query = _dbContext.Roles.AsNoTracking();

      query = !isSystemAdminUser
              ? query.Where(r => r.OrganizationId == organizationId)
              : query.Where(r => r.OrganizationId == null || r.OrganizationId == organizationId);
      return query;
   }
}
