using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs.Responses;

namespace IAM.Infrastructure.QueryRepositories;

public class OrganizationQueryRepository(IamDbContext dbContext) : IOrganizationQueryRepository
{
   private const int DefaultPageNumber = 1;
   private const int DefaultPageSize = 25;
   private const int MaxPageSize = 200;

   private readonly IamDbContext _dbContext = dbContext;

   public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Organizations
          .AsNoTracking()
          .Where(c => c.Id == id)
          .Select(c => new OrganizationDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<PagedResultDto<OrganizationDto>> GetAsync(OrganizationSearchRequest request, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Organizations.AsNoTracking();

      if (request.Type.HasValue)
      {
         query = query.Where(c => c.Type == request.Type.Value);
      }

      if (!string.IsNullOrWhiteSpace(request.Code))
      {
         query = query.Where(c => EF.Functions.ILike(c.Code, $"%{request.Code}%"));
      }

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         query = query.Where(c => EF.Functions.ILike(c.Name, $"%{request.Name}%"));
      }

      if (request.OrganizationId.HasValue)
      {
         query = query.Where(c => c.Id == request.OrganizationId.Value);
      }

      var pageNumber = request.PageNumber < 1 ? DefaultPageNumber : request.PageNumber;
      var pageSize = request.PageSize < 1 ? DefaultPageSize : Math.Min(request.PageSize, MaxPageSize);
      var totalCount = await query.LongCountAsync(cancellationToken);

      var items = await query
         .OrderBy(c => c.Name)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
         .Select(c => new OrganizationDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<OrganizationDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }

   public async Task<IEnumerable<OrganizationDto>> GetAllAsync(CancellationToken cancellationToken = default)
   {
      return await _dbContext.Organizations
          .AsNoTracking()
          .Select(c => new OrganizationDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .ToListAsync(cancellationToken);
   }

   public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Organizations.AnyAsync(c => c.Code == code, cancellationToken);
   }
}
