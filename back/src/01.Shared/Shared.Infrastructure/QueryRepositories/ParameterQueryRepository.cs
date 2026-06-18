using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Mappers;

namespace Shared.Infrastructure.QueryRepositories;

internal class ParameterQueryRepository(SharedDbContext dbContext, IUserContext userContext) : IParameterQueryRepository
{
   private readonly SharedDbContext _dbContext = dbContext;
   private readonly IUserContext _userContext = userContext;

   public async Task<ParameterDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var parameter = await _dbContext.Parameters
         .AsNoTracking()
         .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
      return parameter?.ToParameterDto();
   }

   public async Task<PagedResultDto<ParameterLiteDto>> GetAsync(ParameterSearchRequest request, CancellationToken cancellationToken = default)
   {
      var query = _dbContext.Parameters.AsNoTracking();

      if (!string.IsNullOrWhiteSpace(request.Module))
         query = query.Where(p => p.Module == request.Module);

      if (!string.IsNullOrWhiteSpace(request.Group))
         query = query.Where(p => EF.Functions.ILike(p.Group, $"%{request.Group}%"));

      if (!string.IsNullOrWhiteSpace(request.Name))
         query = query.Where(p => EF.Functions.ILike(p.Name, $"%{request.Name}%"));

      if (!string.IsNullOrWhiteSpace(request.Key))
         query = query.Where(p => EF.Functions.ILike(p.Key, $"%{request.Key}%"));

      if (!string.IsNullOrWhiteSpace(request.Title))
         query = query.Where(p => EF.Functions.ILike(p.Title, $"%{request.Title}%"));

      if (!_userContext.IsSystemAdmin && !_userContext.IsSupportUser)
         query = query.Where(p => p.IsVisible && p.OverrideType != ParameterOverrideType.None);

      var pageNumber = request.PageNumber < 1 ? SharedConst.Pagination.DefaultPageNumber : request.PageNumber;
      var pageSize = request.PageSize < 1 ? SharedConst.Pagination.DefaultPageSize : Math.Min(request.PageSize, SharedConst.Pagination.MaxPageSize);

      var totalCount = await query.LongCountAsync(cancellationToken);

      var items = await query
         .OrderBy(p => p.Module)
         .ThenBy(p => p.Group)
         .ThenBy(p => p.Name)
         .Select(p => new ParameterLiteDto
         {
            Id = p.Id,
            Module = p.Module,
            Group = p.Group,
            Name = p.Name,
            Key = p.Key,
            Title = p.Title,
            Description = p.Description,
            Type = p.Type,
            Value = p.Value,
            ParameterOverrideId = null,
            OverrideType = p.OverrideType,
            IsOverridden = false
         }).ToListAsync(cancellationToken);

      var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

      return new PagedResultDto<ParameterLiteDto>(items, pageNumber, pageSize, totalCount, totalPages);
   }

   public async Task<IEnumerable<ParameterLiteDto>> GetOwnerByAsync(ParameterOverrideType overrideType, CancellationToken cancellationToken = default)
   {
      var query = from param in _dbContext.Parameters.AsNoTracking()
                  join paramOverride in _dbContext.ParameterOverrides on new
                  {
                     ParamId = param.Id,
                     Owner = param.OverrideType == ParameterOverrideType.Organization ? _userContext.OrganizationId : _userContext.UserId
                  }
                  equals new
                  {
                     ParamId = paramOverride.ParameterId,
                     Owner = paramOverride.OwnerId
                  } into overrides
                  from paramOverride in overrides.DefaultIfEmpty()
                  where param.IsVisible && param.OverrideType == overrideType
                  select new { param, paramOverride };


      return await query
         .OrderBy(p => p.param.Module)
         .ThenBy(p => p.param.Group)
         .ThenBy(p => p.param.Title)
         .Select(x => new ParameterLiteDto
         {
            Id = x.param.Id,
            Module = x.param.Module,
            Group = x.param.Group,
            Name = x.param.Name,
            Key = x.param.Key,
            Title = x.param.Title,
            Description = x.param.Description,
            Type = x.param.Type,
            Value = x.paramOverride != null ? x.paramOverride.Value : x.param.Value,
            ParameterOverrideId = x.paramOverride != null ? x.paramOverride.Id : null,
            OverrideType = x.param.OverrideType,
            IsOverridden = x.paramOverride != null
         }).ToListAsync(cancellationToken);
   }

   public async Task<ParameterDto?> GetByModuleGroupAndKeyAsync(string module, string group, string name, CancellationToken cancellationToken = default)
   {
      var parameter = await _dbContext.Parameters
         .AsNoTracking()
         .SingleOrDefaultAsync(p => p.Module == module && p.Group == group && p.Name == name, cancellationToken);
      return parameter?.ToParameterDto();
   }

   public async Task<ParameterValueDto?> GetValueAsync(string key, Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Parameters
              .AsNoTracking()
              .Where(p => p.Key == key)
              .Select(p => new
              {
                 param = p,
                 paramOverride = _dbContext.ParameterOverrides
                      .Where(o => o.ParameterId == p.Id &&
                                 o.OwnerId == (p.OverrideType == ParameterOverrideType.Organization ? organizationId : userId))
                      .FirstOrDefault()
              })
              .Select(x => new ParameterValueDto
              {
                 Id = x.param.Id,
                 Key = x.param.Key,
                 Type = x.param.Type,
                 ParameterOverrideId = x.paramOverride != null ? x.paramOverride.Id : null,
                 Value = x.paramOverride != null ? x.paramOverride.Value : x.param.Value,
                 DefaultValue = x.param.Value,
                 CanBeOverride = x.param.OverrideType != ParameterOverrideType.None,
                 IsOverride = x.paramOverride != null,
                 OverrideType = x.param.OverrideType
              })
              .SingleOrDefaultAsync(cancellationToken);
   }
}
