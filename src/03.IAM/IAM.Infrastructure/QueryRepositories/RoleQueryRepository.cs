using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Myce.Response;
using Shared.Application.Contracts;

namespace IAM.Infrastructure.QueryRepositories;

public class RoleQueryRepository(IamDbContext dbContext) : IRoleQueryRepository
{
   private readonly IamDbContext _dbContext = dbContext;

   public async Task<int> CountRolesByRoleIdsAsync(
    IEnumerable<Guid> ids,
    Guid organizationId,
    IUserContext userContext,
    CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithSecurityContextFilter(organizationId, userContext);

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
       Guid? organizationId,
       IUserContext userContext,
       CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithSecurityContextFilter(organizationId, userContext);

      if (!string.IsNullOrWhiteSpace(name))
      {
         query = query.Where(r => EF.Functions.ILike(r.Name, $"%{name}%"));
      }

      return await query
          .Select(r => r.ToRoleDto())
          .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<Guid>> GetDefaultRolesByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Roles
         .AsNoTracking()
         .Where(r => r.OrganizationId == organizationId && r.IsDefault && r.IsActive)
         .Select(r =>  r.Id)
         .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<Permission>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.UserRoles
          .AsNoTracking()
          .Where(ur => ur.UserId == userId)
          .Where(ur => ur.Role.IsActive)
          .SelectMany(ur => ur.Role.RolePermissions.Select(rf => rf.Permission))
          .Where(permission => permission.IsActive)
          .Distinct()
          .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(
      Guid roleId,
      CancellationToken cancellationToken = default)
   {
      return await _dbContext.RolePermissions
         .AsNoTracking()
         .Where(rp => rp.RoleId == roleId && rp.Role.IsActive && rp.Permission.IsActive)
         .Select(rp => rp.Permission.ToPermissionDto())
         .ToListAsync(cancellationToken);
   }

   public async Task<bool> NameExistsAsync(
      string name,
      Guid? organizationId,
      IUserContext userContext,
      CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithSecurityContextFilter(organizationId, userContext);

      return await query.AnyAsync(r => r.Name == name, cancellationToken);
   }

   private IQueryable<Role> CreateQueryWithSecurityContextFilter(Guid? organizationId, IUserContext userContext)
   {
      var query = _dbContext.Roles.AsNoTracking();

      if (!userContext.IsSystemAdmin)
      {
         //if user is not system admin, we need to filter roles based on user's organization and their assigned roles
         query = from role in query
                 join rp in _dbContext.UserRoles on role.Id equals rp.RoleId
                 where rp.UserId ==  userContext.UserId && 
                       role.OrganizationId == (organizationId.Equals(Guid.Empty) ? null : organizationId)
                 select role;
      }
      else
      {
         query = query.Where(r => r.OrganizationId == null || r.OrganizationId == organizationId);
      }

      return query;
   }
}
