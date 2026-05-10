using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.QueryRepositories;

public class UserQueryRepository(IamDbContext dbContext) : IUserQueryRepository
{
   private readonly IamDbContext _dbContext = dbContext;

   public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Users
          .AsNoTracking()
          .Include(u => u.Organization)
          .Where(u => u.Id == id)
          .Select(u => u.ToUserDto())
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Users
          .AsNoTracking()
          .Include(u => u.Organization)
          .Where(u => u.Email == email)
          .Select(u => u)
          .SingleOrDefaultAsync(cancellationToken);
   }

   public Task<Guid> GetIdByEmailAsync(string email, CancellationToken cancellationToken = default)
   {
      return _dbContext.Users
          .AsNoTracking()
          .Where(u => u.Email == email)
          .Select(u => u.Id)
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<UserPasswordDto?> GetByEmailWithPasswordAsync(string email, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Users
          .AsNoTracking()
          .Include(u => u.Organization)
          .Include(u => u.UserRoles.Where(ur => ur.ExpiresAt == null && ur.Role.IsActive))
          .Where(u => u.Email == email)
          .Select(u => u.ToUserPasswordDto())
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<IEnumerable<UserLiteDto>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Users
          .AsNoTracking()
          .Where(u => u.OrganizationId == organizationId)
          .Select(u => new UserLiteDto
          {
             Id = u.Id,
             Name = u.Name,
             Email = u.Email,
             IsActive = u.IsActive
          })
          .ToListAsync(cancellationToken);
   }
}