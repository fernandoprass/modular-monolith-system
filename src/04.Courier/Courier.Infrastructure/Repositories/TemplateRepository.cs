using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Mappers;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Domain.DTOs.Responses;

namespace Courier.Infrastructure.Repositories;

public class TemplateRepository(CourierDbContext dbContext) : ITemplateRepository, ITemplateWriteRepository
{
   private readonly CourierDbContext _dbContext = dbContext;

   public async Task<PagedResultDto<TemplateLiteDto>> GetAsync(TemplateSearchRequest request, CancellationToken cancellationToken = default)
   {
      var filter = BuildFilter(request);
      var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
      var pageSize = request.PageSize < 1 ? 25 : request.PageSize;
      var skip = (pageNumber - 1) * pageSize;

      var totalCount = (int)await _dbContext.Templates.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

      var templates = await _dbContext.Templates
         .Find(filter)
         .SortBy(t => t.Key)
         .Skip(skip)
         .Limit(pageSize)
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<TemplateLiteDto>(
         templates.Select(t => t.ToTemplateLiteDto()).ToList(),
         pageNumber,
         pageSize,
         totalCount,
         totalPages);
   }

   public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Templates
         .Find(t => t.Id == id)
         .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<Template?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(key))
      {
         return null;
      }

      var normalizedKey = key.ToLowerInvariant().Trim();

      return await _dbContext.Templates
         .Find(t => t.Key == normalizedKey)
         .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<bool> KeyExistsAsync(string key, Guid? excludedId = null, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(key))
      {
         return false;
      }

      var normalizedKey = key.ToLowerInvariant().Trim();
      var builder = Builders<Template>.Filter;
      var filter = builder.Eq(t => t.Key, normalizedKey);

      if (excludedId.HasValue)
      {
         filter &= builder.Ne(t => t.Id, excludedId.Value);
      }

      return await _dbContext.Templates.CountDocumentsAsync(filter, cancellationToken: cancellationToken) > 0;
   }

   public async Task<Guid> AddAsync(Template template, CancellationToken cancellationToken = default)
   {
      await _dbContext.Templates.InsertOneAsync(template, cancellationToken: cancellationToken);
      return template.Id;
   }

   public async Task UpdateAsync(Template template, CancellationToken cancellationToken = default)
   {
      await _dbContext.Templates.ReplaceOneAsync(
         t => t.Id == template.Id,
         template,
         cancellationToken: cancellationToken);
   }

   public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      await _dbContext.Templates.DeleteOneAsync(t => t.Id == id, cancellationToken);
   }

   public async Task<RetentionPolicy?> GetRetentionPolicyByKeyAsync(string key, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(key))
      {
         return null;
      }

      var normalizedKey = key.ToLowerInvariant().Trim();

      return await _dbContext.Templates
         .Find(t => t.Key == normalizedKey && t.Type == TemplateType.Email)
         .Project(t => (RetentionPolicy?)t.RetentionPolicy)
         .SingleOrDefaultAsync(cancellationToken);
   }

   private static FilterDefinition<Template> BuildFilter(TemplateSearchRequest request)
   {
      var builder = Builders<Template>.Filter;
      var filter = builder.Empty;

      if (!string.IsNullOrWhiteSpace(request.Key))
      {
         filter &= builder.Regex(t => t.Key, new BsonRegularExpression(request.Key.Trim(), "i"));
      }

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         filter &= builder.Regex(t => t.Name, new BsonRegularExpression(request.Name.Trim(), "i"));
      }

      if (request.Type.HasValue)
      {
         filter &= builder.Eq(t => t.Type, request.Type.Value);
      }

      return filter;
   }
}
