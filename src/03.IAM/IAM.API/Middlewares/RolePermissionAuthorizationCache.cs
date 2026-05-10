using IAM.Application.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace IAM.API.Middlewares;

public class RolePermissionAuthorizationCache(IMemoryCache cache) : IRolePermissionAuthorizationCache
{
   private const string CacheKeyPrefix = "role_permissions";
   private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);
   private readonly IMemoryCache _cache = cache;

   public async Task<IEnumerable<string>> GetOrCreateAsync(Guid roleId, Func<Task<IEnumerable<string>>> factory)
   {
      var cacheKey = GetCacheKey(roleId);

      return await _cache.GetOrCreateAsync(cacheKey, async entry =>
      {
         entry.AbsoluteExpirationRelativeToNow = CacheDuration;
         return await factory();
      }) ?? [];
   }

   public void Remove(Guid roleId)
   {
      _cache.Remove(GetCacheKey(roleId));
   }

   private static string GetCacheKey(Guid roleId)
   {
      return $"{CacheKeyPrefix}_{roleId}";
   }
}
