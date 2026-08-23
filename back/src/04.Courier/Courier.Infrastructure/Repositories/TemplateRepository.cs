using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Mappers;
using Courier.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;

namespace Courier.Infrastructure.Repositories;

public class TemplateRepository(
   CourierDbContext dbContext,
   IUserContext userContext) : ITemplateRepository, ITemplateWriteRepository
{
   private readonly CourierDbContext _dbContext = dbContext;
   private readonly IUserContext _userContext = userContext;

   public async Task<PagedResultDto<TemplateLiteDto>> GetAsync(TemplateSearchRequest request, CancellationToken cancellationToken = default)
   {
      var language = LanguageOptions.Normalize(_userContext.Language);
      var filter = BuildFilter(request, language);
      var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
      var pageSize = request.PageSize < 1 ? 25 : request.PageSize;
      var skip = (pageNumber - 1) * pageSize;

      var totalCount = (int)await _dbContext.Templates.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

      var templates = await _dbContext.Templates
         .Find(filter)
         .SortBy(t => t.Module)
         .ThenBy(t => t.Key)
         .Skip(skip)
         .Limit(pageSize)
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<TemplateLiteDto>(
         templates.Select(t => t.ToTemplateLiteDto(language)).ToList(),
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

   public async Task<Template?> GetByModuleAndKeyAsync(
      string module,
      string key,
      CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(key))
      {
         return null;
      }

      var normalizedModule = module.ToLowerInvariant().Trim();
      var normalizedKey = key.ToLowerInvariant().Trim();

      return await _dbContext.Templates
         .Find(t => t.Module == normalizedModule && t.Key == normalizedKey)
         .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<bool> KeyExistsAsync(
      string module,
      string key,
      Guid? excludedId = null,
      CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(key))
      {
         return false;
      }

      var normalizedModule = module.ToLowerInvariant().Trim();
      var normalizedKey = key.ToLowerInvariant().Trim();
      var builder = Builders<Template>.Filter;
      var filter = builder.Eq(t => t.Module, normalizedModule)
         & builder.Eq(t => t.Key, normalizedKey);

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

   public async Task<RetentionPolicy?> GetRetentionPolicyByModuleAndKeyAsync(
      string module,
      string key,
      CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(key))
      {
         return null;
      }

      var normalizedModule = module.ToLowerInvariant().Trim();
      var normalizedKey = key.ToLowerInvariant().Trim();

      return await _dbContext.Templates
         .Find(t => t.Module == normalizedModule && t.Key == normalizedKey)
         .Project(t => (RetentionPolicy?)t.RetentionPolicy)
         .SingleOrDefaultAsync(cancellationToken);
   }

   private static FilterDefinition<Template> BuildFilter(TemplateSearchRequest request, string language)
   {
      var builder = Builders<Template>.Filter;
      var filter = builder.Empty;

      if (!string.IsNullOrWhiteSpace(request.Module))
      {
         filter &= builder.Regex(t => t.Module, new BsonRegularExpression(request.Module.Trim(), "i"));
      }

      if (!string.IsNullOrWhiteSpace(request.Key))
      {
         filter &= builder.Regex(t => t.Key, new BsonRegularExpression(request.Key.Trim(), "i"));
      }

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         var translationBuilder = Builders<TemplateTranslation>.Filter;
         var translationFilter = translationBuilder.Eq(t => t.Language, LanguageOptions.Normalize(language))
            & translationBuilder.Regex(t => t.Name, new BsonRegularExpression(request.Name.Trim(), "i"));

         filter &= builder.ElemMatch("translations", translationFilter);
      }

      if (request.Severity.HasValue)
      {
         filter &= builder.Eq(t => t.Severity, request.Severity.Value);
      }

      return filter;
   }
}
