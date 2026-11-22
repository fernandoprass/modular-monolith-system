using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Interfaces;
using Shared.Domain.Mappers;

namespace Shared.Infrastructure.QueryRepositories;

internal class LookupQueryRepository(SharedDbContext dbContext) : ILookupQueryRepository
{
   public async Task<LookupDto?> GetByIdAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default)
   {
      var lookup = await dbContext.Lookups.AsNoTracking()
         .Include(x => x.Translations)
         .SingleOrDefaultAsync(x => x.Id == id && (x.OrganizationId == null || x.OrganizationId == organizationId), cancellationToken);
      
      return lookup?.ToLookupDto();
   }

   public async Task<PagedResultDto<LookupLiteDto>> GetAsync(LookupSearchRequest request, Guid organizationId,
      CancellationToken cancellationToken = default)
   {
      var language = LanguageOptions.Normalize(request.LanguageCode ?? SharedConst.System.DefaultLanguage);
      var query = dbContext.Lookups.AsNoTracking().Where(x => x.OrganizationId == null || x.OrganizationId == organizationId);
      if (!string.IsNullOrWhiteSpace(request.Type))
      {
         var type = request.Type.Trim().ToLowerInvariant();
         query = query.Where(x => x.Type == type);
      }

      if (request.RootsOnly)
      {
         query = query.Where(x => x.ParentId == null);
      }
      else if (request.ParentId.HasValue)
      {
         query = query.Where(x => x.ParentId == request.ParentId);
      }
      if (request.IsActive.HasValue)
      {
         query = query.Where(x => x.IsActive == request.IsActive);
      }

      var pageSize = Math.Min(request.PageSize, SharedConst.Pagination.MaxPageSize);
      var count = await query.LongCountAsync(cancellationToken);
      var items = await query.OrderBy(x => x.Type)
         .ThenBy(x => x.Code).ThenBy(x => x.Id)
         .Skip(checked((request.PageNumber - 1) * pageSize)).Take(pageSize)
         .Select(x => new
         {
            x.Id, x.Type, x.Code, x.IsActive, x.ParentId, x.OrganizationId,
            Translation = x.Translations.Where(t => t.IsActive &&
               (t.LanguageCode == language || t.LanguageCode == SharedConst.System.DefaultLanguage))
               .OrderByDescending(t => t.LanguageCode == language).FirstOrDefault()
         })
         .Select(x => new LookupLiteDto(x.Id, x.Type, x.Code, x.IsActive, x.ParentId, x.OrganizationId,
            x.Translation == null ? x.Code : x.Translation.Value,
            x.Translation == null ? null : x.Translation.Description,
            x.Translation == null ? null : x.Translation.LanguageCode))
         .ToListAsync(cancellationToken);
      return new(items, request.PageNumber, pageSize, count, (int)Math.Ceiling(count / (double)pageSize));
   }
}
