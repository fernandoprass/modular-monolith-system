using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.QueryRepositories;

public class PermissionQueryRepository(IamDbContext dbContext) : IPermissionQueryRepository
{
   private readonly IamDbContext _dbContext = dbContext;

   public async Task<PermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var permission = await _dbContext.Permissions
         .AsNoTracking()
         .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

      return permission?.ToPermissionDto();
   }

   public async Task<IEnumerable<PermissionDto>> GetByParams(PermissionSearchRequest request, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Permissions.AsNoTracking();

      if (request.roleId != null && request.roleId != Guid.Empty)
         query = from p in query
                 join rp in _dbContext.RolePermissions on p.Id equals rp.PermissionId
                 where rp.RoleId == request.roleId
                 select p;

      if (!string.IsNullOrWhiteSpace(request.Module))
         query = query.Where(p => EF.Functions.ILike(p.Module, $"%{request.Module}%"));

      if (!string.IsNullOrWhiteSpace(request.Group))
         query = query.Where(p => EF.Functions.ILike(p.Group, $"%{request.Group}%"));

      if (!string.IsNullOrWhiteSpace(request.Name))
         query = query.Where(p => EF.Functions.ILike(p.Name, $"%{request.Name}%"));

      if (!request.IncludeInactive)
         query = query.Where(p => p.IsActive);

      return await query
         .Select(p => p.ToPermissionDto())
         .ToListAsync(cancellationToken);
   }

   public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Permissions
         .AnyAsync(p => p.Code == code.ToLowerInvariant(), cancellationToken);
   }

   public async Task<bool> CodeExistsAsync(string code, Guid excludedId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Permissions
         .AnyAsync(p => p.Id != excludedId && p.Code == code.ToLowerInvariant(), cancellationToken);
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
