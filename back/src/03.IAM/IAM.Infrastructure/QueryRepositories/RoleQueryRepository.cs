using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;
using static IAM.Domain.IamPermission;

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
         query = query.Where(role => role.UserRoles.Any(userRole => userRole.UserId == request.UserId.Value));
      }

      if (request.IsActive.HasValue)
      {
         query = query.Where(r => r.IsActive == request.IsActive.Value);
      }

      return await query
          .OrderBy(r => r.Name)
          .Select(r => r.ToRoleDto())
          .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<Guid>> GetDefaultRolesByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
   {
      organizationId = _userContext.IsSystemAdmin ? organizationId : _userContext.OrganizationId;

      return await _dbContext.Roles
         .AsNoTracking()
         .Where(r => r.OrganizationId == organizationId && r.IsDefault && r.IsActive)
         .Select(r =>  r.Id)
         .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<UserRoleDto>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      string systemUserName = "System";

      userId = GetUserWithSecurityContext(userId);

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

   public async Task<IEnumerable<RoleDto>> GetAvailableRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      userId = GetUserWithSecurityContext(userId);

      // 1. Get a flat subquery list of all RoleIds the user currently possesses
      var assignedRoleIds = _dbContext.UserRoles
          .Where(ur => ur.UserId == userId)
          .Select(ur => ur.RoleId);



      // 2. Query the master Roles table, filtering out the ones they already have
      var query = _dbContext.Roles.AsNoTracking()
          .Where(r => r.IsActive && 
                      (r.OrganizationId == _userContext.OrganizationId ||
                       r.OrganizationId == (_userContext.IsSystemAdmin ? null : _userContext.OrganizationId)) &&
                      !assignedRoleIds.Contains(r.Id)) // Core logic change
          .OrderBy(r => r.Name)
          .Select(r => new RoleDto(
              r.Id,          
              r.Name,
              r.Description,
              r.IsActive,
              r.IsDefault,
              r.OrganizationId
          ));

      return await query.ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<Permission>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      var now = DateTime.UtcNow;

      userId = GetUserWithSecurityContext(userId);

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

   public async Task<IEnumerable<RolePermissionCodeDto>> GetPermissionCodesByRoleIdsAsync(
      IEnumerable<Guid> roleIds, 
      CancellationToken cancellationToken = default)
   {
      return await _dbContext.RolePermissions
         .AsNoTracking()
         .Where(rp => roleIds.Contains(rp.RoleId) && rp.Role.IsActive && rp.Permission.IsActive)
         .Select(rp => new RolePermissionCodeDto(rp.RoleId, rp.Permission.Code))
         .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<string>> GetPermissionCodesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      var now = DateTime.UtcNow;

      userId = GetUserWithSecurityContext(userId);

      return await _dbContext.UserRoles
          .AsNoTracking()
          .Where(ur => ur.UserId == userId &&
                       ur.Role.IsActive &&
                       ur.StartsAt <= now &&
                       (ur.ExpiresAt == null || ur.ExpiresAt >= now))
          .SelectMany(ur => ur.Role.RolePermissions.Where(rp => rp.Permission.IsActive).Select(rp => rp.Permission.Code))
          .Distinct()
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

   private Guid GetUserWithSecurityContext(Guid userId)
   {
      userId = _userContext.IsSystemAdmin || _userContext.IsOrganizationAdmin ? userId : _userContext.UserId;
      return userId;
   }

   private IQueryable<Role> CreateQueryWithSecurityContextFilter()
   {
      var query = _dbContext.Roles.AsNoTracking();

      // 1. System Admins bypass security filters entirely
      if (_userContext.IsSystemAdmin)
      {
         return query.Where(r => r.OrganizationId == null || r.OrganizationId == _userContext.OrganizationId);
      }

      // 2. Regular users must be explicitly assigned to the role.
      //    Organization admins also get roles belonging to their organization.
      return query.Where(role =>
          _dbContext.UserRoles.Any(ru => ru.RoleId == role.Id && ru.UserId == _userContext.UserId) ||
          (_userContext.IsOrganizationAdmin && role.OrganizationId == _userContext.OrganizationId));
   }
}
