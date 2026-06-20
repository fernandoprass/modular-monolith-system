using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Responses;

namespace IAM.Infrastructure.QueryRepositories;

public class PermissionQueryRepository(IamDbContext dbContext, IUserContext userContext) : IPermissionQueryRepository
{
   private readonly IamDbContext _dbContext = dbContext;
   private readonly IUserContext _userContext = userContext;

   public async Task<PermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var permission = await _dbContext.Permissions
         .AsNoTracking()
         .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

      return permission?.ToPermissionDto();
   }

   public async Task<PagedResultDto<PermissionDto>> GetByParams(PermissionSearchRequest request, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Permissions.AsNoTracking();

      if (!_userContext.IsSystemAdmin)
      {
            query = from p in query
                    join rp in _dbContext.RolePermissions on p.Id equals rp.PermissionId
                    where rp.RoleId == (request.roleId.HasValue ? request.roleId : rp.RoleId) &&
                          p.RolePermissions.Any(rp => rp.Role.UserRoles.Any(ur => ur.UserId == _userContext.UserId))
                    select p;
      }

      if (!string.IsNullOrWhiteSpace(request.Module))
         query = query.Where(p => p.Module == request.Module);

      if (!string.IsNullOrWhiteSpace(request.Resource))
         query = query.Where(p => p.Resource == request.Resource);

      if (!string.IsNullOrWhiteSpace(request.Action))
         query = query.Where(p => EF.Functions.ILike(p.Action, $"%{request.Action}%"));

      if (!string.IsNullOrWhiteSpace(request.Title))
         query = query.Where(p => EF.Functions.ILike(p.Title, $"%{request.Title}%"));

      if (!request.IncludeInactive)
         query = query.Where(p => p.IsActive);

      var pageNumber = request.PageNumber < 1 ? SharedConst.Pagination.DefaultPageNumber : request.PageNumber;
      var pageSize = request.PageSize < 1 ? SharedConst.Pagination.DefaultPageSize : Math.Min(request.PageSize, SharedConst.Pagination.MaxPageSize);
      var totalCount = await query.LongCountAsync(cancellationToken);

      var items = await query
         .OrderBy(p => p.Module)
         .ThenBy(p => p.Resource)
         .ThenBy(p => p.Action)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
         .Select(p => p.ToPermissionDto())
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<PermissionDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }

   public async Task<IEnumerable<PermissionDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Permissions
         .AsNoTracking()
         .Include(p => p.RolePermissions)
         .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId))
         .OrderBy(p => p.Module)
         .ThenBy(p => p.Resource)
         .ThenBy(p => p.Action)
         .Select(p => p.ToPermissionDto());

      return await query.ToListAsync(cancellationToken);
   }

   /// <summary>
   /// Retrieves a list of active permissions available to be assigned to a specific target role.
   /// </summary>
   /// <remarks>
   /// This method filters the global permission catalog based on two strict criteria:
   /// <list type="bullet">
   /// <item><description>The current requesting user must currently possess the permission through their active, non-expired roles.</description></item>
   /// <item><description>The target role (<paramref name="roleId"/>) must not already have this permission associated with it.</description></item>
   /// </list>
   /// </remarks>
   /// <param name="roleId">The unique identifier of the target role receiving the new permission.</param>
   /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
   /// <returns>A read-only collection of <see cref="PermissionDto"/> objects sorted by module, resource, and action.</returns>
   public async Task<IEnumerable<PermissionDto>> GetByAvailablePermissionsRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
   {
      var now = DateTime.UtcNow;

      var existingPermissionIds = _dbContext.RolePermissions
          .Where(rp => rp.RoleId == roleId)
          .Select(rp => rp.PermissionId);

      var query = _dbContext.UserRoles
          .AsNoTracking()
          .Where(ur => ur.UserId == _userContext.UserId &&
                       ur.Role.IsActive &&
                       ur.StartsAt <= now &&
                       (ur.ExpiresAt == null || ur.ExpiresAt >= now))
          .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
          .Where(p => p.IsActive && !existingPermissionIds.Contains(p.Id))
          .Distinct()
          .OrderBy(p => p.Module)
          .ThenBy(p => p.Resource)
          .ThenBy(p => p.Action)
          .Select(p => new PermissionDto(
              p.Id,
              p.Module,
              p.Resource,
              p.Action,
              p.Code,
              p.Title,
              p.Description,
              p.IsActive
          ));

      return await query.ToListAsync(cancellationToken);
   }

   public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Permissions
         .AnyAsync(p => p.Code == code.ToLowerInvariant(), cancellationToken);
   }

   public async Task<bool> CodeExistsAsync(string code, Guid excludedId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Permissions
         .AnyAsync(p => p.Id != excludedId && p.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase), cancellationToken);
   }

   public async Task<PermissionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Permissions
         .AsNoTracking()
         .Where(p => p.Code == code.ToLowerInvariant())
         .Select(p => p.ToPermissionDto())
         .SingleOrDefaultAsync(cancellationToken);
   }
}
