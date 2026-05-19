using Shared.Domain.DTOs.Responses;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Repositories;

// Used when Redis is not configured, so parameter services keep working with database-only reads.
public class ParameterNullCacheRepository : IParameterCacheRespository
{
   public Task<string?> GetAsync(string key, Guid userOwnerId, Guid userId, CancellationToken cancellationToken = default)
   {
      return Task.FromResult<string?>(null);
   }

   public Task SetAsync(ParameterValueDto parameter, Guid ownerId, CancellationToken cancellationToken = default)
   {
      return Task.CompletedTask;
   }

   public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
   {
      return Task.CompletedTask;
   }

   public Task RemoveOverrideAsync(string key, Guid ownerId, CancellationToken cancellationToken = default)
   {
      return Task.CompletedTask;
   }
}
