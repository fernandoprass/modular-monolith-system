using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using MongoDB.Driver;
using Shared.Domain.DTOs.Responses;

namespace Courier.Infrastructure.Repositories;

public class EmailRepository(CourierDbContext dbContext) : IEmailRepository
{
   private readonly CourierDbContext _dbContext = dbContext;

   public async Task<PagedResultDto<EmailLiteDto>> GetAsync(EmailSearchRequest request, CancellationToken cancellationToken = default)
   {
      var filter = BuildFilter(request);
      var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
      var pageSize = request.PageSize < 1 ? 25 : request.PageSize;
      var skip = (pageNumber - 1) * pageSize;

      var totalCount = (int)await _dbContext.Emails.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

      var items = await _dbContext.Emails
         .Find(filter)
         .SortByDescending(e => e.CreatedAt)
         .Skip(skip)
         .Limit(pageSize)
         .Project(e => new EmailLiteDto(
            e.Id,
            e.Module,
            e.Feature,
            e.Recipient,
            e.Subject,
            e.Status))
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<EmailLiteDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }

   public async Task<Email?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Emails
         .Find(e => e.Id == id)
         .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<Email?> ClaimNextPendingAsync(DateTime utcNow, CancellationToken cancellationToken = default)
   {
      var builder = Builders<Email>.Filter;
      var filter = builder.Eq(e => e.Status, EmailStatus.Pending)
         & builder.Lte(e => e.NextAttemptAt, utcNow);

      var update = Builders<Email>.Update.Set(e => e.Status, EmailStatus.Processing);
      var options = new FindOneAndUpdateOptions<Email>
      {
         Sort = Builders<Email>.Sort.Ascending(e => e.CreatedAt),
         ReturnDocument = ReturnDocument.After
      };

      return await _dbContext.Emails.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
   }

   public async Task<Guid> AddAsync(Email email, CancellationToken cancellationToken = default)
   {
      await _dbContext.Emails.InsertOneAsync(email, cancellationToken: cancellationToken);
      return email.Id;
   }

   public async Task UpdateAsync(Email email, CancellationToken cancellationToken = default)
   {
      await _dbContext.Emails.ReplaceOneAsync(e => e.Id == email.Id, email, cancellationToken: cancellationToken);
   }

   private static FilterDefinition<Email> BuildFilter(EmailSearchRequest request)
   {
      var builder = Builders<Email>.Filter;
      var filter = builder.Empty;

      filter &= builder.Gte(e => e.CreatedAt, request.DateFrom);
      filter &= builder.Lte(e => e.CreatedAt, request.DateTo);

      if (request.OrganizationId.HasValue)
      {
         filter &= builder.Eq(e => e.OrganizationId, request.OrganizationId.Value);
      }

      if (request.UserId.HasValue)
      {
         filter &= builder.Eq(e => e.UserId, request.UserId.Value);
      }

      if (!string.IsNullOrWhiteSpace(request.Module))
      {
         filter &= builder.Regex(e => e.Module, new MongoDB.Bson.BsonRegularExpression(request.Module.Trim(), "i"));
      }

      if (!string.IsNullOrWhiteSpace(request.Feature))
      {
         filter &= builder.Regex(e => e.Feature, new MongoDB.Bson.BsonRegularExpression(request.Feature.Trim(), "i"));
      }

      if (!string.IsNullOrWhiteSpace(request.Subject))
      {
         filter &= builder.Regex(e => e.Subject, new MongoDB.Bson.BsonRegularExpression(request.Subject.Trim(), "i"));
      }

      if (!string.IsNullOrWhiteSpace(request.Recipient))
      {
         filter &= builder.Regex(e => e.Recipient, new MongoDB.Bson.BsonRegularExpression(request.Recipient.Trim(), "i"));
      }

      return filter;
   }
}
