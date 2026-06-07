using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;

namespace IAM.Infrastructure.QueryRepositories;

public class UserQueryRepository(IamDbContext dbContext, IUserContext userContext) : IUserQueryRepository
{
   private const int DefaultPageNumber = 1;
   private const int DefaultPageSize = 25;
   private const int MaxPageSize = 200;

   private readonly IamDbContext _dbContext = dbContext;
   private readonly IUserContext _userContext;

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

   public async Task<PagedResultDto<UserLiteDto>> GetAsync(UserSearchRequest request, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Users.AsNoTracking();

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         query = query.Where(u => EF.Functions.ILike(u.Name, $"%{request.Name}%"));
      }

      if (!string.IsNullOrWhiteSpace(request.Email))
      {
         query = query.Where(u => EF.Functions.ILike(u.Email, $"%{request.Email}%"));
      }

      if (request.OrganizationId.HasValue)
      {
         query = query.Where(u => u.OrganizationId == request.OrganizationId.Value);
      }

      var pageNumber = request.PageNumber < 1 ? DefaultPageNumber : request.PageNumber;
      var pageSize = request.PageSize < 1 ? DefaultPageSize : Math.Min(request.PageSize, MaxPageSize);
      var totalCount = await query.LongCountAsync(cancellationToken);

      var items = await query
         .OrderBy(u => u.Name)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
          .Select(u => new UserLiteDto
          {
             Id = u.Id,
             Name = u.Name,
             Email = u.Email,
             IsActive = u.IsActive
          })
          .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<UserLiteDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }
}