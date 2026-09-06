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
      string normalizedEmail = NormalizeEmail(email);

      return await _dbContext.Users
          .AsNoTracking()
          .Include(u => u.Organization)
          .Where(u => u.Email == normalizedEmail)
          .Select(u => u)
          .SingleOrDefaultAsync(cancellationToken);
   }

   public Task<Guid> GetIdByEmailAsync(string email, CancellationToken cancellationToken = default)
   {
      string normalizedEmail = NormalizeEmail(email);

      return _dbContext.Users
          .AsNoTracking()
          .Where(u => u.Email == normalizedEmail)
          .Select(u => u.Id)
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<UserPasswordDto?> GetByEmailWithPasswordAsync(string email, CancellationToken cancellationToken = default)
   {
      string normalizedEmail = NormalizeEmail(email);

      return await _dbContext.Users
          .AsNoTracking()
          .Where(u => u.Email == normalizedEmail)
          .Select(u => new UserPasswordDto
          {
             Id = u.Id,
             Name = u.Name,
             Email = u.Email,
             PasswordHash = u.PasswordHash,
             IsActive = u.IsActive,
             IsSystemAdmin = u.IsSystemAdmin,
             IsSupportUser = u.IsSupportUser,
             IsOrganizationAdmin = u.IsOrganizationAdmin,
             NumFailedLoginAttempts = u.NumFailedLoginAttempts,
             CreatedAt = u.CreatedAt,
             EmailVerifiedAt = u.EmailVerifiedAt,
             LastLoginAt = u.LastLoginAt,
             Language = u.Language,
             LockedOutUntil = u.LockedOutUntil,

             OrganizationId = u.OrganizationId,
             OrganizationName = u.Organization.Name,
             OrganizationIsActive = u.Organization.IsActive,

             RoleIds = u.UserRoles
                  .Where(ur => ur.Role.IsActive &&
                               (ur.ExpiresAt == null || ur.ExpiresAt >= DateTime.UtcNow))
                  .Select(ur => ur.RoleId)
          })
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
            Language = u.Language,
            IsActive = u.IsActive
         })
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<UserLiteDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }

   public async Task<IEnumerable<UserLookupDto>> GetLookupAsync(UserLookupRequest request, CancellationToken cancellationToken = default)
   {
      var query = CreateQueryWithSecurityContextFilter(_userContext.OrganizationId);

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

   private IQueryable<User> CreateQueryWithSecurityContextFilter(Guid organizationId)
   {
      var query = _dbContext.Users.AsNoTracking();

      organizationId = _userContext.IsSystemAdmin ? organizationId : _userContext.OrganizationId;

      query = query.Where(u => u.OrganizationId == organizationId);

      return query;
   }

   private static string NormalizeEmail(string email)
   {
      return email.ToLowerInvariant().Trim();
   }

}
