using Microsoft.Extensions.Caching.Distributed;
using Shared.Application.Contracts;
using Shared.Domain;
using System.Text.Json;

namespace Shared.Infrastructure.Authorization;

public class DistributedRolePermissionCache(IDistributedCache cache) : IRolePermissionCache, IRolePermissionCacheInvalidator
{
   private readonly IDistributedCache _cache = cache;

   public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(string role, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(role))
      {
         return [];
      }

      var cachedPermissions = await _cache.GetStringAsync(GetCacheKey(role), cancellationToken);

      if (string.IsNullOrWhiteSpace(cachedPermissions))
      {
         return [];
      }

      return JsonSerializer.Deserialize<IReadOnlyCollection<string>>(cachedPermissions) ?? [];
   }

   public async Task SetPermissionsAsync(
      string role,
      IEnumerable<string> permissions,
      DateTime expiresAt,
      CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(role))
      {
         return;
      }

      var cacheOptions = new DistributedCacheEntryOptions
      {
         AbsoluteExpiration = new DateTimeOffset(expiresAt)
      };

      var permissionSet = permissions
         .Where(permission => !string.IsNullOrWhiteSpace(permission))
         .Distinct(StringComparer.OrdinalIgnoreCase)
         .ToArray();

      await _cache.SetStringAsync(
         GetCacheKey(role),
         JsonSerializer.Serialize(permissionSet),
         cacheOptions,
         cancellationToken);
   }

   public async Task RemovePermissionsAsync(string role, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(role))
      {
         return;
      }

      await _cache.RemoveAsync(GetCacheKey(role), cancellationToken);
   }

   public async Task RemoveAsync(Guid roleId, CancellationToken cancellationToken = default)
   {
      await RemovePermissionsAsync(roleId.ToString(), cancellationToken);
   }

   private static string GetCacheKey(string role)
   {
      return $"{SharedConst.Redis.CacheKeyPrefixForRole}{role}";
   }
}
