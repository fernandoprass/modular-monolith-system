using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Domain.DTOs.Responses;

namespace Courier.Infrastructure.Repositories;

public class NotificationRepository(CourierDbContext dbContext) : INotificationRepository
{
   private readonly CourierDbContext _dbContext = dbContext;

   public async Task<PagedResultDto<NotificationLiteDto>> GetAsync(
      NotificationSearchRequest request,
      CancellationToken cancellationToken = default)
   {
      var filter = BuildFilter(request);
      var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
      var pageSize = request.PageSize < 1 ? 25 : request.PageSize;
      var skip = (pageNumber - 1) * pageSize;

      var totalCount = (int)await _dbContext.Notifications.CountDocumentsAsync(
         filter,
         cancellationToken: cancellationToken);

      var items = await _dbContext.Notifications
         .Find(filter)
         .SortByDescending(n => n.CreatedAt)
         .Skip(skip)
         .Limit(pageSize)
         .Project(n => new NotificationLiteDto(
            n.Id,
            n.Module,
            n.Feature,
            n.Title,
            n.Message,
            n.ActionLink,
            n.Status,
            n.CreatedAt,
            n.ReadAt))
         .ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<NotificationLiteDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }

   public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Notifications
         .Find(n => n.Id == id)
         .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<int> GetUnreadCountAsync(
      Guid organizationId,
      Guid userId,
      CancellationToken cancellationToken = default)
   {
      var filter = Builders<Notification>.Filter.Eq(n => n.OrganizationId, organizationId)
         & Builders<Notification>.Filter.Eq(n => n.UserId, userId)
         & Builders<Notification>.Filter.Eq(n => n.Status, NotificationStatus.Unread);

      return (int)await _dbContext.Notifications.CountDocumentsAsync(
         filter,
         cancellationToken: cancellationToken);
   }

   public async Task<Guid> AddAsync(Notification notification, CancellationToken cancellationToken = default)
   {
      await _dbContext.Notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);
      return notification.Id;
   }

   public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);

      var result = await _dbContext.Notifications.DeleteOneAsync(filter, cancellationToken: cancellationToken);
      return result.DeletedCount > 0;
   }

   public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
   {
      await _dbContext.Notifications.ReplaceOneAsync(
         n => n.Id == notification.Id,
         notification,
         cancellationToken: cancellationToken);
   }

   private static FilterDefinition<Notification> BuildFilter(NotificationSearchRequest request)
   {
      var builder = Builders<Notification>.Filter;
      var filter = builder.Gte(n => n.CreatedAt, request.DateFrom)
         & builder.Lte(n => n.CreatedAt, request.DateTo);

      if (request.OrganizationId.HasValue)
      {
         filter &= builder.Eq(n => n.OrganizationId, request.OrganizationId.Value);
      }

      if (request.UserId.HasValue)
      {
         filter &= builder.Eq(n => n.UserId, request.UserId.Value);
      }

      if (!string.IsNullOrWhiteSpace(request.Module))
      {
         filter &= builder.Regex(n => n.Module, new BsonRegularExpression(request.Module.Trim(), "i"));
      }

      if (!string.IsNullOrWhiteSpace(request.Title))
      {
         filter &= builder.Regex(n => n.Title, new BsonRegularExpression(request.Title.Trim(), "i"));
      }

      if (request.Status.HasValue)
      {
         filter &= builder.Eq(n => n.Status, request.Status.Value);
      }

      return filter;
   }
}
