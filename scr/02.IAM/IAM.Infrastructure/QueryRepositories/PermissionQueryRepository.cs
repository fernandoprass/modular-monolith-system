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
         .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

      return permission?.ToPermissionDto();
   }

   public async Task<IEnumerable<PermissionDto>> GetAllAsync(
      string? module, 
      string? group, 
      string? name, 
      CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Permissions.AsNoTracking();

      if (!string.IsNullOrWhiteSpace(module))
         query = query.Where(p => EF.Functions.ILike(p.Module, $"%{module}%"));

      if (!string.IsNullOrWhiteSpace(group))
         query = query.Where(p => EF.Functions.ILike(p.Group, $"%{group}%"));

      if (!string.IsNullOrWhiteSpace(name))
         query = query.Where(p => EF.Functions.ILike(p.Name, $"%{name}%"));

      return await query
         .Select(p => p.ToPermissionDto())
         .ToListAsync(cancellationToken);
   }

   public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Permissions
         .AnyAsync(p => p.Code == code.ToLowerInvariant(), cancellationToken);
   }
}
