using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;

namespace IAM.Infrastructure.QueryRepositories;

public class RoleQueryRepository(IamDbContext dbContext, IUserContext userContext) : IRoleQueryRepository
{
   private readonly IamDbContext _dbContext = dbContext;
   private readonly IUserContext _userContext = userContext;

   public async Task<int> CountRolesByRoleIdsAsync(
    IEnumerable<Guid> ids,
    Guid organizationId,
    CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithSecurityContextFilter();

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

   public async Task<IEnumerable<RoleDto>> GetAsync(RoleSearchRequest request, CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithSecurityContextFilter();

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         query = query.Where(r => EF.Functions.ILike(r.Name, $"%{request.Name}%"));
      }

      if (request.UserId.HasValue)
      {
         query = query.Where(r => r.UserRoles.Any(ur => ur.UserId == request.UserId.Value));
      }

      if (request.OrganizationId.HasValue)
      {
         query = query.Where(r => r.OrganizationId == request.OrganizationId.Value);
      }

      if (request.IsActive.HasValue)
      {
         query = query.Where(r => r.IsActive == request.IsActive.Value);
      }

      return await query
          .Select(r => r.ToRoleDto())
          .OrderBy(r => r.Name)
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

   public async Task<IEnumerable<UserRoleDto>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      string systemUserName = "System";

      var query = _dbContext.UserRoles.AsNoTracking()
          .OrderBy(ur => ur.Role.Name)
          .Where(ur => ur.UserId == userId)
          .Select(ur => new UserRoleDto(
              ur.Id,
              ur.RoleId,
              ur.Role.Name,
              ur.Role.IsActive,
              ur.Role.IsDefault,
              ur.StartsAt,
              ur.ExpiresAt,
              AssignedBy: ur.CreatedBy != Guid.Empty
                   ? _dbContext.Users.Where(u => u.Id == ur.CreatedBy).Select(u => u.Name).FirstOrDefault() ?? systemUserName
                   : systemUserName,
              AssignedAt: ur.Role.CreatedAt
          ));

      return await query.ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<Permission>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      var now = DateTime.UtcNow;

      return await _dbContext.UserRoles
          .AsNoTracking()
          .Where(ur => ur.UserId == userId &&
                       ur.Role.IsActive &&
                       ur.StartsAt <= now &&
                       (ur.ExpiresAt == null || ur.ExpiresAt >= now))
          .SelectMany(ur => ur.Role.RolePermissions.Select(rf => rf.Permission))
          .Where(permission => permission.IsActive)
          .Distinct()
          .OrderBy(permission => permission.Module)
          .ThenBy(permission => permission.Resource)
          .ThenBy(permission => permission.Action)
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
      CancellationToken cancellationToken = default)
   {
      return await _dbContext.Roles.AsNoTracking()
         .AnyAsync(r => r.Name == name && r.OrganizationId == organizationId, cancellationToken);
   }

   private IQueryable<Role> CreateQueryWithSecurityContextFilter()
   {
      var query = _dbContext.Roles.AsNoTracking();

      // 1. System Admins bypass security filters entirely
      if (_userContext.IsSystemAdmin)
      {
         return query;
      }

      // 2. Regular users must be explicitly assigned to the role.
      //    Organization admins also get roles belonging to their organization.
      return query.Where(role =>
          _dbContext.UserRoles.Any(ru => ru.RoleId == role.Id && ru.UserId == _userContext.UserId) ||
          (_userContext.IsOrganizationAdmin && role.OrganizationId == _userContext.UserOwnerId));
   }
}
