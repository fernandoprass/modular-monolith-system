using MongoDB.Bson;
using MongoDB.Driver;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.Entities;
using Sentinel.Domain.QueryRepositories;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;
using System.Text.RegularExpressions;

namespace Sentinel.Infrastructure.QueryRepositories;

public class SentinelLogQueryRepository(SentinelDbContext dbContext) : ISentinelLogQueryRepository
{
   private const int DefaultPageNumber = 1;
   private const int DefaultPageSize = 50;
   private const int MaxPageSize = 200;

   private readonly SentinelDbContext _dbContext = dbContext;

   public async Task<PagedResultDto<AuditLogLiteDto>> GetAuditLogsByParamsAsync(
      AuditLogSearchRequest request,
      IUserContext userContext,
      CancellationToken cancellationToken = default)
   {
      var filter = BuildAuditLogFilter(request, userContext);
      var (pageNumber, pageSize) = NormalizePaging(request.PageNumber, request.PageSize);

      var totalCount = await _dbContext.AuditLogs.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
      var logs = await _dbContext.AuditLogs
         .Find(filter)
         .SortByDescending(log => log.CreatedAt)
         .Skip((pageNumber - 1) * pageSize)
         .Limit(pageSize)
         .ToListAsync(cancellationToken);

      var items = logs.Select(a => new AuditLogLiteDto(
         a.Id,
         a.Module,
         a.Feature,
         a.Action,
         a.PrivacyLevel,
         a.Description,
         a.CreatedAt,
         a.ExpiresAt,
         a.UserId,
         a.TargetId)).ToList();

      return new PagedResultDto<AuditLogLiteDto>(
         items,
         pageNumber,
         pageSize,
         (int)totalCount,
         GetTotalPages(totalCount, pageSize));
   }

   public async Task<PagedResultDto<SystemLogLiteDto>> GetSystemLogsByParamsAsync(
      SystemLogSearchRequest request,
      IUserContext userContext,
      CancellationToken cancellationToken = default)
   {
      var filter = BuildSystemLogFilter(request, userContext);
      var (pageNumber, pageSize) = NormalizePaging(request.PageNumber, request.PageSize);

      var totalCount = await _dbContext.SystemLogs.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
      var logs = await _dbContext.SystemLogs
         .Find(filter)
         .SortByDescending(log => log.CreatedAt)
         .Skip((pageNumber - 1) * pageSize)
         .Limit(pageSize)
         .ToListAsync(cancellationToken);

      var items = logs.Select(s => new SystemLogLiteDto(
         s.Id,
         s.Level,
         s.Status,
         s.Module,
         s.Message,
         s.CreatedAt,
         s.ExpiresAt,
         s.RequestId,
         s.UserId,
         s.OrganizationId)).ToList();

      return new PagedResultDto<SystemLogLiteDto>(
         items,
         pageNumber,
         pageSize,
         (int)totalCount,
         GetTotalPages(totalCount, pageSize));
   }

   private static FilterDefinition<AuditLog> BuildAuditLogFilter(AuditLogSearchRequest request, IUserContext userContext)
   {
      var builder = Builders<AuditLog>.Filter;
      var filters = new List<FilterDefinition<AuditLog>>();

      if (!userContext.IsSystemAdmin)
         filters.Add(builder.Eq(a => a.OrganizationId, userContext.OrganizationId));
      else if (request.OrganizationId.HasValue)
         filters.Add(builder.Eq(a => a.OrganizationId, request.OrganizationId.Value));

      if (request.UserId.HasValue)
         filters.Add(builder.Eq(a => a.UserId, request.UserId.Value));

      if (!string.IsNullOrWhiteSpace(request.Module))
         filters.Add(builder.Regex(a => a.Module, Contains(request.Module)));

      if (!string.IsNullOrWhiteSpace(request.Feature))
         filters.Add(builder.Regex(a => a.Feature, Contains(request.Feature)));

      if (!string.IsNullOrWhiteSpace(request.Action))
         filters.Add(builder.Regex(a => a.Action, Contains(request.Action)));

      if (request.PrivacyLevel.HasValue)
         filters.Add(builder.Eq(a => a.PrivacyLevel, request.PrivacyLevel.Value));

      if (request.TargetId.HasValue)
         filters.Add(builder.Eq(a => a.TargetId, request.TargetId.Value));

      if (request.From.HasValue)
         filters.Add(builder.Gte(a => a.CreatedAt, request.From.Value));

      if (request.To.HasValue)
         filters.Add(builder.Lte(a => a.CreatedAt, request.To.Value));

      return filters.Count == 0 ? builder.Empty : builder.And(filters);
   }

   private static FilterDefinition<SystemLog> BuildSystemLogFilter(SystemLogSearchRequest request, IUserContext userContext)
   {
      var builder = Builders<SystemLog>.Filter;
      var filters = new List<FilterDefinition<SystemLog>>();

      if (!userContext.IsSystemAdmin)
         filters.Add(builder.Eq(s => s.OrganizationId, userContext.OrganizationId));
      else if (request.OrganizationId.HasValue)
         filters.Add(builder.Eq(s => s.OrganizationId, request.OrganizationId.Value));

      if (request.UserId.HasValue)
         filters.Add(builder.Eq(s => s.UserId, request.UserId.Value));

      if (request.Level.HasValue)
         filters.Add(builder.Eq(s => s.Level, request.Level.Value));

      if (request.Status.HasValue)
         filters.Add(builder.Eq(s => s.Status, request.Status.Value));

      if (!string.IsNullOrWhiteSpace(request.Module))
         filters.Add(builder.Regex(s => s.Module, Contains(request.Module)));

      if (!string.IsNullOrWhiteSpace(request.RequestId))
         filters.Add(builder.Eq(s => s.RequestId, request.RequestId));

      if (request.From.HasValue)
         filters.Add(builder.Gte(s => s.CreatedAt, request.From.Value));

      if (request.To.HasValue)
         filters.Add(builder.Lte(s => s.CreatedAt, request.To.Value));

      return filters.Count == 0 ? builder.Empty : builder.And(filters);
   }

   private static BsonRegularExpression Contains(string value)
   {
      return new BsonRegularExpression($".*{Regex.Escape(value)}.*", "i");
   }

   private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
   {
      var normalizedPageNumber = pageNumber < 1 ? DefaultPageNumber : pageNumber;
      var normalizedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

      return (normalizedPageNumber, normalizedPageSize);
   }

   private static int GetTotalPages(long totalCount, int pageSize)
   {
      return totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
   }

   public async Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id, IUserContext userContext, CancellationToken cancellationToken = default)
   {
      var builder = Builders<AuditLog>.Filter;
      var filter = builder.Eq(a => a.Id, id);

      if (!userContext.IsSystemAdmin)
      {
         filter &= builder.Eq(a => a.OrganizationId, userContext.OrganizationId);
      }

      var auditLog = await _dbContext.AuditLogs
         .Find(filter)
         .SingleOrDefaultAsync(cancellationToken);

      return auditLog == null
         ? null
         : new AuditLogDto(
            auditLog.Id,
            auditLog.Module,
            auditLog.Feature,
            auditLog.Action,
            auditLog.PrivacyLevel,
            auditLog.Description,
            auditLog.CreatedAt,
            auditLog.ExpiresAt,
            auditLog.UserId,
            auditLog.OrganizationId,
            auditLog.TargetId,
            auditLog.IpAddress,
            auditLog.UserAgent,
            auditLog.Metadata);
   }

   public async Task<SystemLogDto?> GetSystemLogByIdAsync(Guid id, IUserContext userContext, CancellationToken cancellationToken = default)
   {
      var builder = Builders<SystemLog>.Filter;
      var filter = builder.Eq(s => s.Id, id);

      if (!userContext.IsSystemAdmin)
      {
         filter &= builder.Eq(s => s.OrganizationId, userContext.OrganizationId);
      }

      var systemLog = await _dbContext.SystemLogs
         .Find(filter)
         .SingleOrDefaultAsync(cancellationToken);

      return systemLog == null
         ? null
         : new SystemLogDto(
            systemLog.Id,
            systemLog.Level,
            systemLog.Status,
            systemLog.Module,
            systemLog.Message,
            systemLog.Exception,
            systemLog.StackTrace,
            systemLog.CreatedAt,
            systemLog.ExpiresAt,
            systemLog.RequestId,
            systemLog.UserId,
            systemLog.OrganizationId,
            systemLog.PropertiesJson);
   }
}
