using Shared.Domain;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Interfaces;
using StackExchange.Redis;

namespace Shared.Infrastructure.Repositories;

public class ParameterRedisCacheRepository(IConnectionMultiplexer redis) : IParameterCacheRespository
{
   private const string DefaultField = "default";
   private readonly IDatabase _database = redis.GetDatabase();

   public async Task<string?> GetAsync(string key, Guid userOwnerId, Guid userId, CancellationToken cancellationToken = default)
   {
      var redisKey = GetCacheKey(key);
      var keyType = await _database.KeyTypeAsync(redisKey);

      if (keyType == RedisType.None)
      {
         return null;
      }

      if (keyType == RedisType.String)
      {
         var value = await _database.StringGetAsync(redisKey);
         return value.IsNull ? null : value.ToString();
      }

      if (keyType != RedisType.Hash)
      {
         return null;
      }

      var values = await _database.HashGetAsync(
         redisKey,
         [DefaultField, userOwnerId.ToString(), userId.ToString()]);

      if (!values[2].IsNull)
      {
         return values[2].ToString();
      }

      return values[1].IsNull ? null : values[1].ToString();
   }

   public async Task SetAsync(ParameterValueDto parameter, Guid ownerId, CancellationToken cancellationToken = default)
   {
      var redisKey = GetCacheKey(parameter.Key);

      if (!parameter.CanBeOverride)
      {
         await _database.StringSetAsync(redisKey, parameter.Value);
         return;
      }

      await _database.HashSetAsync(redisKey, DefaultField, parameter.DefaultValue);

      if (parameter.IsOverride)
      {
         await _database.HashSetAsync(redisKey, ownerId.ToString(), parameter.Value);
      }
   }

   public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
   {
      await _database.KeyDeleteAsync(GetCacheKey(key));
   }

   public async Task RemoveOverrideAsync(string key, Guid ownerId, CancellationToken cancellationToken = default)
   {
      await _database.HashDeleteAsync(GetCacheKey(key), ownerId.ToString());
   }

   private static RedisKey GetCacheKey(string key)
   {
      return $"{SharedConst.Redis.CacheKeyPrefixForParameter}{key}";
   }
}
