using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shared.Infrastructure.Authorization;
using System.Text;
using System.Text.Json;

namespace Shared.Infrastructure.Tests.Authorization;

public class DistributedRolePermissionCacheTests
{
   [Theory]
   [InlineData("role-a", "iam.users.list")]
   [InlineData("role-b", "iam.users.list", "iam.users.create")]
   public async Task GetPermissionsAsync_ShouldDeserializeCachedPermissions(string role, params string[] permissions)
   {
      var cache = Substitute.For<IDistributedCache>();
      var service = new DistributedRolePermissionCache(cache);

      cache.GetAsync($"perm:{role}", Arg.Any<CancellationToken>())
         .Returns(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(permissions)));

      var result = await service.GetPermissionsAsync(role, TestContext.Current.CancellationToken);

      result.Should().BeEquivalentTo(permissions);
   }

   [Theory]
   [InlineData("role-a", "iam.users.list")]
   [InlineData("role-b", "iam.users.list", "iam.users.create")]
   public async Task SetPermissionsAsync_ShouldSerializePermissionsAndUseAbsoluteExpiration(string role, params string[] permissions)
   {
      var cache = Substitute.For<IDistributedCache>();
      var service = new DistributedRolePermissionCache(cache);
      var expiresAt = DateTime.UtcNow.AddMinutes(15);

      byte[]? cachedBytes = null;
      DistributedCacheEntryOptions? cacheOptions = null;

      await cache.SetAsync(
         $"perm:{role}",
         Arg.Do<byte[]>(value => cachedBytes = value),
         Arg.Do<DistributedCacheEntryOptions>(options => cacheOptions = options),
         Arg.Any<CancellationToken>());

      await service.SetPermissionsAsync(role, permissions, expiresAt, TestContext.Current.CancellationToken);

      var cachedPermissions = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(Encoding.UTF8.GetString(cachedBytes!));
      cachedPermissions.Should().BeEquivalentTo(permissions);
      cacheOptions!.AbsoluteExpiration.Should().Be(new DateTimeOffset(expiresAt));
   }

   [Fact]
   public async Task RemoveAsync_ShouldRemoveRolePermissions()
   {
      var cache = Substitute.For<IDistributedCache>();
      var service = new DistributedRolePermissionCache(cache);
      var roleId = Guid.NewGuid();

      await service.RemoveAsync(roleId, TestContext.Current.CancellationToken);

      await cache.Received(1).RemoveAsync($"perm:{roleId}", Arg.Any<CancellationToken>());
   }

   [Fact]
   public void AddSharedAuthorization_ShouldRequireRedisConnectionString()
   {
      var services = new ServiceCollection();
      var configuration = new ConfigurationBuilder().Build();

      Action act = () => services.AddSharedAuthorization(configuration);

      act.Should().Throw<InvalidOperationException>()
         .WithMessage("*Redis connection string*");
   }

   [Fact]
   public void AddSharedAuthorization_ShouldRegisterRedisDistributedCache_WhenRedisConnectionStringExists()
   {
      var services = new ServiceCollection();
      var configuration = new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["ConnectionStrings:Redis"] = "localhost:6379,abortConnect=false"
         })
         .Build();

      services.AddSharedAuthorization(configuration);

      using var provider = services.BuildServiceProvider();
      var cache = provider.GetRequiredService<IDistributedCache>();

      cache.GetType().FullName.Should().Contain("StackExchangeRedis");
   }
}
