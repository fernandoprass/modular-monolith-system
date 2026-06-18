using Shared.Domain.DTOs.Responses;

namespace Shared.Domain.Interfaces;

public interface IParameterCacheRespository
{
   Task<string?> GetAsync(string key, Guid OrganizationId, Guid userId, CancellationToken cancellationToken = default);
   Task SetAsync(ParameterValueDto parameter, Guid ownerId, CancellationToken cancellationToken = default);
   Task RemoveAsync(string key, CancellationToken cancellationToken = default);
   Task RemoveOverrideAsync(string key, Guid ownerId, CancellationToken cancellationToken = default);
}
