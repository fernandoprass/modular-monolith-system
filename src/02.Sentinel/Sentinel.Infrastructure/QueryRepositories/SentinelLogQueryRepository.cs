using Microsoft.EntityFrameworkCore;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.QueryRepositories;
using Shared.Application.Contracts;

namespace Sentinel.Infrastructure.QueryRepositories;

public class SentinelLogQueryRepository(SentinelDbContext dbContext) : ISentinelLogQueryRepository
{
   private const int DefaultPageNumber = 1;
   private const int DefaultPageSize = 50;
   private const int MaxPageSize = 200;

   private readonly SentinelDbContext _dbContext = dbContext;

   public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsByParamsAsync(AuditLogSearchRequest request, IUserContext userContext, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.AuditLogs.AsNoTracking();

      if (!userContext.IsSystemAdmin)
      {
         query = query.Where(a => a.OrganizationId == userContext.UserOwnerId);
      }
      else if (request.OrganizationId.HasValue)
      {
         query = query.Where(a => a.OrganizationId == request.OrganizationId.Value);
      }

      if (request.UserId.HasValue)
         query = query.Where(a => a.UserId == request.UserId.Value);

      if (!string.IsNullOrWhiteSpace(request.Module))
         query = query.Where(a => EF.Functions.ILike(a.Module, $"%{request.Module}%"));

      if (!string.IsNullOrWhiteSpace(request.Feature))
         query = query.Where(a => EF.Functions.ILike(a.Feature, $"%{request.Feature}%"));

      if (!string.IsNullOrWhiteSpace(request.Action))
         query = query.Where(a => EF.Functions.ILike(a.Action, $"%{request.Action}%"));

      if (request.PrivacyLevel.HasValue)
         query = query.Where(a => a.PrivacyLevel == request.PrivacyLevel.Value);

      if (request.TargetId.HasValue)
         query = query.Where(a => a.TargetId == request.TargetId.Value);

      if (request.From.HasValue)
         query = query.Where(a => a.Timestamp >= request.From.Value);

      if (request.To.HasValue)
         query = query.Where(a => a.Timestamp <= request.To.Value);

      var (pageNumber, pageSize) = NormalizePaging(request.PageNumber, request.PageSize);
      var totalCount = await query.CountAsync(cancellationToken);

      var items = await query
         .OrderByDescending(a => a.Timestamp)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
         .Select(a => new AuditLogDto(
            a.Id,
            a.Timestamp,
            a.Module,
            a.Feature,
            a.Action,
            a.PrivacyLevel,
            a.Description,
            a.UserId,
            a.OrganizationId,
            a.TargetId,
            a.IpAddress,
            a.UserAgent,
            a.Metadata))
         .ToListAsync(cancellationToken);

      return new PagedResultDto<AuditLogDto>(
         items,
         pageNumber,
         pageSize,
         totalCount,
         GetTotalPages(totalCount, pageSize));
   }

   public async Task<PagedResultDto<SystemLogDto>> GetSystemLogsByParamsAsync(SystemLogSearchRequest request, IUserContext userContext, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.SystemLogs.AsNoTracking();

      if (!userContext.IsSystemAdmin)
      {
         query = query.Where(s => s.OrganizationId == userContext.UserOwnerId);
      }
      else if (request.OrganizationId.HasValue)
      {
         query = query.Where(s => s.OrganizationId == request.OrganizationId.Value);
      }

      if (request.UserId.HasValue)
         query = query.Where(s => s.UserId == request.UserId.Value);

      if (request.Level.HasValue)
         query = query.Where(s => s.Level == request.Level.Value);

      if (request.Status.HasValue)
         query = query.Where(s => s.Status == request.Status.Value);

      if (!string.IsNullOrWhiteSpace(request.Source))
         query = query.Where(s => EF.Functions.ILike(s.Source, $"%{request.Source}%"));

      if (!string.IsNullOrWhiteSpace(request.RequestId))
         query = query.Where(s => s.RequestId == request.RequestId);

      if (request.From.HasValue)
         query = query.Where(s => s.Timestamp >= request.From.Value);

      if (request.To.HasValue)
         query = query.Where(s => s.Timestamp <= request.To.Value);

      var (pageNumber, pageSize) = NormalizePaging(request.PageNumber, request.PageSize);
      var totalCount = await query.CountAsync(cancellationToken);

      var items = await query
         .OrderByDescending(s => s.Timestamp)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
         .Select(s => new SystemLogDto(
            s.Id,
            s.Timestamp,
            s.Level,
            s.Status,
            s.Source,
            s.Message,
            s.Exception,
            s.StackTrace,
            s.RequestId,
            s.UserId,
            s.OrganizationId,
            s.PropertiesJson))
         .ToListAsync(cancellationToken);

      return new PagedResultDto<SystemLogDto>(
         items,
         pageNumber,
         pageSize,
         totalCount,
         GetTotalPages(totalCount, pageSize));
   }

   private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
   {
      var normalizedPageNumber = pageNumber < 1 ? DefaultPageNumber : pageNumber;
      var normalizedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

      return (normalizedPageNumber, normalizedPageSize);
   }

   private static int GetTotalPages(int totalCount, int pageSize)
   {
      return totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
   }
}
