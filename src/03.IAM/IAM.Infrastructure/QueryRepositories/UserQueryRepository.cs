using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Responses;

namespace IAM.Infrastructure.QueryRepositories;

public class UserQueryRepository(IamDbContext dbContext, IUserContext userContext) : IUserQueryRepository
{
   private readonly IamDbContext _dbContext = dbContext;
   private readonly IUserContext _userContext = userContext;

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
      var query = CreateQueryWithSecurityContextFilter(request.OrganizationId);

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         query = query.Where(u => EF.Functions.ILike(u.Name, $"%{request.Name}%"));
      }

      if (!string.IsNullOrWhiteSpace(request.Email))
      {
         query = query.Where(u => EF.Functions.ILike(u.Email, $"%{request.Email}%"));
      }

      if (request.IsActive.HasValue)
      {
         query = query.Where(u => u.IsActive == request.IsActive.Value);
      }

      var pageNumber = request.PageNumber < 1 ? SharedConst.Pagination.DefaultPageNumber : request.PageNumber;
      var pageSize = request.PageSize < 1 ? SharedConst.Pagination.DefaultPageSize : Math.Min(request.PageSize, SharedConst.Pagination.MaxPageSize);
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

   public async Task<IEnumerable<UserLookupDto>> GetLookupAsync(UserLookupRequest request, CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithSecurityContextFilter(request.OrganizationId);

      if (request.Id.HasValue)
      {
         query = query.Where(u => u.Id == request.Id.Value);
      }

      if (!string.IsNullOrWhiteSpace(request.Search))
      {
         query = query.Where(u => EF.Functions.ILike(u.Name, $"%{request.Search}%"));
      }

      if (!request.IncludeInactive)
      {
         query = query.Where(u => u.IsActive);
      }

      return await query
          .OrderBy(u => u.Name)
          .Select(u => new UserLookupDto(
             u.Id,
             u.Name,
             u.IsActive,
             u.OrganizationId
             ))
          .Take(request.Take)
          .ToListAsync(cancellationToken);
   }

   private IQueryable<User> CreateQueryWithSecurityContextFilter(Guid? organizationId)
   {
      var query = _dbContext.Users.AsNoTracking();

      if(!_userContext.IsSystemAdmin)
      {
         query = query.Where(u => u.OrganizationId == _userContext.UserOwnerId);
      }

      if (organizationId.HasValue)
      {
         query = query.Where(u => u.OrganizationId == organizationId.Value);
      }

      return query;
   }

}
