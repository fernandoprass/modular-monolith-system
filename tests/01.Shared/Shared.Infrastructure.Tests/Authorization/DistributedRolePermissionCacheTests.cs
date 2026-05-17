using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Authorization;

namespace Shared.Infrastructure.Tests.Authorization;

public class DistributedRolePermissionCacheTests
{
   [Fact]
   public async Task SetPermissionsAsync_ShouldStorePermissionsUntilExpiration()
   {
      IDistributedCache cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
      var service = new DistributedRolePermissionCache(cache);
      var roleId = Guid.NewGuid().ToString();

      await service.SetPermissionsAsync(roleId, ["iam.users.list"], DateTime.UtcNow.AddMinutes(5), TestContext.Current.CancellationToken);

      var permissions = await service.GetPermissionsAsync(roleId, TestContext.Current.CancellationToken);

      permissions.Should().Contain("iam.users.list");
   }

   [Fact]
   public async Task RemoveAsync_ShouldRemoveRolePermissions()
   {
      IDistributedCache cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
      var service = new DistributedRolePermissionCache(cache);
      var roleId = Guid.NewGuid();

      await service.SetPermissionsAsync(roleId.ToString(), ["iam.users.list"], DateTime.UtcNow.AddMinutes(5), TestContext.Current.CancellationToken);
      await service.RemoveAsync(roleId, TestContext.Current.CancellationToken);

      var permissions = await service.GetPermissionsAsync(roleId.ToString(), TestContext.Current.CancellationToken);

      permissions.Should().BeEmpty();
   }
}
