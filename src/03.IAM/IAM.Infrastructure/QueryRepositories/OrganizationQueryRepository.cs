using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;

namespace IAM.Infrastructure.QueryRepositories;

public class OrganizationQueryRepository(IamDbContext dbContext, IUserContext userContext) : IOrganizationQueryRepository
{
   private const int DefaultPageNumber = 1;
   private const int DefaultPageSize = 25;
   private const int MaxPageSize = 200;

   private readonly IamDbContext _dbContext = dbContext;
   private readonly IUserContext _userContext;

   public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Organizations
          .AsNoTracking()
          .Where(c => c.Id == id)
          .Select(c => new OrganizationDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.DefaultLanguage, c.IsActive))
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<PagedResultDto<OrganizationDto>> GetAsync(OrganizationSearchRequest request, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Organizations.AsNoTracking();

      if (request.Type.HasValue)
      {
         query = query.Where(org => org.Type == request.Type.Value);
      }

      if (!string.IsNullOrWhiteSpace(request.Code))
      {
         query = query.Where(org => EF.Functions.ILike(org.Code, $"%{request.Code}%"));
      }

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         query = query.Where(org => EF.Functions.ILike(org.Name, $"%{request.Name}%"));
      }

      if (request.OrganizationId.HasValue)
      {
         query = query.Where(org => org.Id == request.OrganizationId.Value);
      }

      var pageNumber = request.PageNumber < 1 ? DefaultPageNumber : request.PageNumber;
      var pageSize = request.PageSize < 1 ? DefaultPageSize : Math.Min(request.PageSize, MaxPageSize);
      var totalCount = await query.LongCountAsync(cancellationToken);

      var items = await query
         .OrderBy(org => org.Name)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
         .Select(org => new OrganizationDto(org.Id, org.Type, org.Code, org.Name, org.Description, org.DefaultLanguage, org.IsActive))
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<OrganizationDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }

   public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Organizations.AnyAsync(c => c.Code == code, cancellationToken);
   }
}
