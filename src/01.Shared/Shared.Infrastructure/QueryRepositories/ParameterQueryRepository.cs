using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Mappers;

namespace Shared.Infrastructure.QueryRepositories;

internal class ParameterQueryRepository(SharedDbContext dbContext) : IParameterQueryRepository
{
   private readonly SharedDbContext _dbContext = dbContext;

   public async Task<ParameterDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var parameter = await _dbContext.Parameters
         .AsNoTracking()
         .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
      return parameter?.ToParameterDto();
   }

   public async Task<IEnumerable<ParameterLiteDto>> GetAllAsync(ParameterSearchRequestInternal request, CancellationToken cancellationToken = default)
   {
      var query = from param in _dbContext.Parameters.AsNoTracking()
                  join paramOverride in _dbContext.ParameterOverrides on new
                  {
                     ParamId = param.Id,
                     Owner = param.OverrideType == ParameterOverrideType.UserOwnerId ? request.UserOwnerId : request.UserId
                  }
                  equals new
                  {
                     ParamId = paramOverride.ParameterId,
                     Owner = paramOverride.OwnerId
                  } into overrides
                  from paramOverride in overrides.DefaultIfEmpty()
                  select new { param, paramOverride };

      if (!string.IsNullOrWhiteSpace(request.Module))
         query = query.Where(x => x.param.Module == request.Module);

      if (!string.IsNullOrWhiteSpace(request.Group))
         query = query.Where(x => x.param.Group == request.Group);

      if (!string.IsNullOrWhiteSpace(request.Name))
         query = query.Where(x => x.param.Name == request.Name);

      if (!string.IsNullOrWhiteSpace(request.Key))
         query = query.Where(x => x.param.Key == request.Key);

      if (!string.IsNullOrWhiteSpace(request.Description))
         query = query.Where(x => x.param.Description.Contains(request.Description));

      if (!request.IsSystemAdmin)
         query = query.Where(x => x.param.IsVisible);

      return await query.Select(x => new ParameterLiteDto
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

   public async Task<ParameterValueDto?> GetValueAsync(string key, Guid userOwnerId, Guid userId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.Parameters
              .AsNoTracking()
              .Where(p => p.Key == key)
              .Select(p => new
              {
                 Parameter = p,
                 OverrideValue = _dbContext.ParameterOverrides
                      .Where(o => o.ParameterId == p.Id &&
                                 o.OwnerId == (p.OverrideType == ParameterOverrideType.UserOwnerId ? userOwnerId : userId))
                      .Select(o => o.Value)
                      .FirstOrDefault()
              })
              .Select(x => new ParameterValueDto
              {
                 Key = x.Parameter.Key,
                 Type = x.Parameter.Type,
                 Value = x.OverrideValue ?? x.Parameter.Value,
                 DefaultValue = x.Parameter.Value,
                 CanBeOverride = x.Parameter.OverrideType != ParameterOverrideType.None,
                 IsOverride = x.OverrideValue != null,
                 OverrideType = x.Parameter.OverrideType
              })
              .SingleOrDefaultAsync(cancellationToken);
   }
}
