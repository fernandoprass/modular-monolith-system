using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Mappers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Courier.Infrastructure.Repositories;

public class EmailTemplateRepository(CourierDbContext dbContext) : IEmailTemplateRepository, IEmailTemplateWriteRepository
{
   private readonly CourierDbContext _dbContext = dbContext;

   public async Task<PagedResultDto<EmailTemplateDto>> GetAsync(EmailTemplateSearchRequest request, CancellationToken cancellationToken = default)
   {
      var filter = BuildFilter(request);
      var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
      var pageSize = request.PageSize < 1 ? 25 : request.PageSize;
      var skip = (pageNumber - 1) * pageSize;

      var totalCount = (int)await _dbContext.EmailTemplates.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

      var templates = await _dbContext.EmailTemplates
         .Find(filter)
         .SortBy(t => t.Key)
         .Skip(skip)
         .Limit(pageSize)
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<EmailTemplateDto>(
         templates.Select(t => t.ToEmailTemplateDto()),
         pageNumber,
         pageSize,
         totalCount,
         totalPages);
   }

   public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.EmailTemplates
         .Find(t => t.Id == id)
         .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<bool> KeyExistsAsync(string key, Guid? excludedId = null, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(key))
      {
         return false;
      }

      var normalizedKey = key.ToLowerInvariant().Trim();
      var builder = Builders<EmailTemplate>.Filter;
      var filter = builder.Eq(t => t.Key, normalizedKey);

      if (excludedId.HasValue)
      {
         filter &= builder.Ne(t => t.Id, excludedId.Value);
      }

      return await _dbContext.EmailTemplates.CountDocumentsAsync(filter, cancellationToken: cancellationToken) > 0;
   }

   public async Task<Guid> AddAsync(EmailTemplate template, CancellationToken cancellationToken = default)
   {
      await _dbContext.EmailTemplates.InsertOneAsync(template, cancellationToken: cancellationToken);
      return template.Id;
   }

   public async Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
   {
      await _dbContext.EmailTemplates.ReplaceOneAsync(
         t => t.Id == template.Id,
         template,
         cancellationToken: cancellationToken);
   }

   public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      await _dbContext.EmailTemplates.DeleteOneAsync(t => t.Id == id, cancellationToken);
   }

   public async Task<EmailRetentionPolicy?> GetRetentionPolicyByKeyAsync(string key, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(key))
      {
         return null;
      }

      var normalizedKey = key.ToLowerInvariant().Trim();

      return await _dbContext.EmailTemplates
         .Find(t => t.Key == normalizedKey)
         .Project(t => (EmailRetentionPolicy?)t.RetentionPolicy)
         .SingleOrDefaultAsync(cancellationToken);
   }

   private static FilterDefinition<EmailTemplate> BuildFilter(EmailTemplateSearchRequest request)
   {
      var builder = Builders<EmailTemplate>.Filter;
      var filter = builder.Empty;

      if (!string.IsNullOrWhiteSpace(request.Key))
      {
         filter &= builder.Regex(t => t.Key, new BsonRegularExpression(request.Key.Trim(), "i"));
      }

      return filter;
   }
}
