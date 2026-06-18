using FluentAssertions;
using NSubstitute;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;
using Shared.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Shared.Infrastructure.Tests.Repositories;

public class ParameterRedisCacheRepositoryTests
{
   private readonly IDatabase _database;
   private readonly IConnectionMultiplexer _redis;
   private readonly ParameterRedisCacheRepository _cache;

   public ParameterRedisCacheRepositoryTests()
   {
      _database = Substitute.For<IDatabase>();
      _redis = CreateRedis(_database);
      _cache = new ParameterRedisCacheRepository(_redis);
   }

   [Fact]
   public async Task FirstTimeRetrievalOfStaticParameter_ShouldReturnNull_WhenKeyDoesNotExist()
   {
      _database.KeyTypeAsync("param:System.MaxRetry").Returns(RedisType.None);

      var value = await _cache.GetAsync("System.MaxRetry", Guid.CreateVersion7(), Guid.CreateVersion7(), TestContext.Current.CancellationToken);

      value.Should().BeNull();
      await _database.DidNotReceive().StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>());
      await _database.DidNotReceive().HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task SubsequentRetrievalOfStaticParameter_ShouldReturnStringValue_WhenKeyIsString()
   {
      _database.KeyTypeAsync("param:System.MaxRetry").Returns(RedisType.String);
      _database.StringGetAsync("param:System.MaxRetry", Arg.Any<CommandFlags>()).Returns("3");

      var value = await _cache.GetAsync("System.MaxRetry", Guid.CreateVersion7(), Guid.CreateVersion7(), TestContext.Current.CancellationToken);

      value.Should().Be("3");
      await _database.Received(1).StringGetAsync("param:System.MaxRetry", Arg.Any<CommandFlags>());
      await _database.DidNotReceive().HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task RetrievalOfOverridableParameterWithNoExistingCache_ShouldReturnNull_WhenHashDoesNotExist()
   {
      _database.KeyTypeAsync("param:UI.Theme").Returns(RedisType.None);

      var value = await _cache.GetAsync("UI.Theme", Guid.CreateVersion7(), Guid.CreateVersion7(), TestContext.Current.CancellationToken);

      value.Should().BeNull();
      await _database.DidNotReceive().HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task EfficiencyCheckForUserUsingDefaultValues_ShouldReturnNull_WhenOwnerAndUserFieldsDoNotExist()
   {
      var ownerId = Guid.CreateVersion7();
      var userId = Guid.CreateVersion7();

      _database.KeyTypeAsync("param:UI.Theme").Returns(RedisType.Hash);
      var expectedFields = new RedisValue[] { "default", ownerId.ToString(), userId.ToString() };

      _database.HashGetAsync(
            "param:UI.Theme",
            Arg.Is<RedisValue[]>(fields => fields.SequenceEqual(expectedFields)),
            Arg.Any<CommandFlags>())
         .Returns([(RedisValue)"Blue", RedisValue.Null, RedisValue.Null]);

      var value = await _cache.GetAsync("UI.Theme", ownerId, userId, TestContext.Current.CancellationToken);

      value.Should().BeNull();
      await _database.Received(1).HashGetAsync("param:UI.Theme", Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task GetAsync_ShouldReturnNull_WhenHashHasDefaultAndDifferentUserOverrideButRequestedUserIsMissing()
   {
      var user1Id = Guid.CreateVersion7();
      var user2Id = Guid.CreateVersion7();
      var ownerId = Guid.CreateVersion7();

      _database.KeyTypeAsync("param:UI.Theme").Returns(RedisType.Hash);

      _database.HashGetAsync(
            "param:UI.Theme",
            Arg.Any<RedisValue[]>(),
            Arg.Any<CommandFlags>())
         .Returns([(RedisValue)"Blue", RedisValue.Null, RedisValue.Null]);

      var value = await _cache.GetAsync("UI.Theme", ownerId, user2Id, TestContext.Current.CancellationToken);

      value.Should().BeNull();
      await _database.Received(1).HashGetAsync(
         "param:UI.Theme",
         Arg.Is<RedisValue[]>(fields =>
            fields.Contains("default") &&
            fields.Contains(ownerId.ToString()) &&
            fields.Contains(user2Id.ToString()) &&
            !fields.Contains(user1Id.ToString())),
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task OverrideLookupWithWarmGlobalCache_ShouldReturnUserOverride_WhenUserFieldExists()
   {
      var ownerId = Guid.CreateVersion7();
      var userId = Guid.CreateVersion7();

      _database.KeyTypeAsync("param:UI.Theme").Returns(RedisType.Hash);
      var expectedFields = new RedisValue[] { "default", ownerId.ToString(), userId.ToString() };

      _database.HashGetAsync(
            "param:UI.Theme",
            Arg.Is<RedisValue[]>(fields => fields.SequenceEqual(expectedFields)),
            Arg.Any<CommandFlags>())
         .Returns([(RedisValue)"Blue", RedisValue.Null, (RedisValue)"Dark"]);

      var value = await _cache.GetAsync("UI.Theme", ownerId, userId, TestContext.Current.CancellationToken);

      value.Should().Be("Dark");
      await _database.Received(1).HashGetAsync("param:UI.Theme", Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task OverrideLookupWithWarmGlobalCache_ShouldReturnUserOverride_WhenOwnerAndUserFieldsExist()
   {
      var ownerId = Guid.CreateVersion7();
      var userId = Guid.CreateVersion7();

      _database.KeyTypeAsync("param:UI.Theme").Returns(RedisType.Hash);
      _database.HashGetAsync(
            "param:UI.Theme",
            Arg.Any<RedisValue[]>(),
            Arg.Any<CommandFlags>())
         .Returns([(RedisValue)"Blue", (RedisValue)"Black", (RedisValue)"Green"]);

      var value = await _cache.GetAsync("UI.Theme", ownerId, userId, TestContext.Current.CancellationToken);

      value.Should().Be("Green");
      await _database.Received(1).HashGetAsync("param:UI.Theme", Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task OverrideLookupWithWarmGlobalCache_ShouldReturnOwnerOverride_WhenOwnerFieldExists()
   {
      var ownerId = Guid.CreateVersion7();
      var userId = Guid.CreateVersion7();

      _database.KeyTypeAsync("param:UI.Theme").Returns(RedisType.Hash);
      _database.HashGetAsync(
            "param:UI.Theme",
            Arg.Any<RedisValue[]>(),
            Arg.Any<CommandFlags>())
         .Returns([(RedisValue)"Blue", (RedisValue)"Dark", RedisValue.Null]);

      var value = await _cache.GetAsync("UI.Theme", ownerId, userId, TestContext.Current.CancellationToken);

      value.Should().Be("Dark");
      await _database.Received(1).HashGetAsync("param:UI.Theme", Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task FirstTimeRetrievalOfStaticParameter_ShouldStoreStaticParameterAsString()
   {
      await _cache.SetAsync(new ParameterValueDto
      {
         Key = "System.MaxRetry",
         Value = "3",
         CanBeOverride = false
      }, Guid.CreateVersion7(), TestContext.Current.CancellationToken);

      _database.ReceivedCalls()
         .Should()
         .ContainSingle(call =>
            call.GetMethodInfo().Name == nameof(IDatabase.StringSetAsync) &&
            call.GetArguments()[0].Equals((RedisKey)"param:System.MaxRetry") &&
            call.GetArguments()[1].Equals((RedisValue)"3"));
   }

   [Fact]
   public async Task EfficiencyCheckForUserUsingDefaultValues_ShouldNotStoreOwnerField_WhenDefaultValueIsUsed()
   {
      var ownerId = Guid.CreateVersion7();

      await _cache.SetAsync(new ParameterValueDto
      {
         Key = "UI.Theme",
         Value = "Blue",
         DefaultValue = "Blue",
         CanBeOverride = true,
         IsOverride = false,
         OverrideType = ParameterOverrideType.OrganizationId
      }, ownerId, TestContext.Current.CancellationToken);

      await _database.Received(1).HashSetAsync("param:UI.Theme", "default", "Blue", Arg.Any<When>(), Arg.Any<CommandFlags>());
      await _database.DidNotReceive().HashSetAsync("param:UI.Theme", ownerId.ToString(), Arg.Any<RedisValue>(), Arg.Any<When>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task RetrievalOfOverridableParameterWithNoExistingCache_ShouldStoreDefaultAndOwnerField_WhenOverrideExists()
   {
      var ownerId = Guid.CreateVersion7();

      await _cache.SetAsync(new ParameterValueDto
      {
         Key = "UI.Theme",
         Value = "Dark",
         DefaultValue = "Blue",
         CanBeOverride = true,
         IsOverride = true,
         OverrideType = ParameterOverrideType.OrganizationId
      }, ownerId, TestContext.Current.CancellationToken);

      await _database.Received(1).HashSetAsync("param:UI.Theme", "default", "Blue", Arg.Any<When>(), Arg.Any<CommandFlags>());
      await _database.Received(1).HashSetAsync("param:UI.Theme", ownerId.ToString(), "Dark", Arg.Any<When>(), Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task RemoveOverrideAsync_ShouldRemoveOnlyOwnerField()
   {
      var ownerId = Guid.CreateVersion7();

      await _cache.RemoveOverrideAsync("UI.Theme", ownerId, TestContext.Current.CancellationToken);

      await _database.Received(1).HashDeleteAsync("param:UI.Theme", ownerId.ToString(), Arg.Any<CommandFlags>());
      await _database.DidNotReceive().KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>());
   }

   private static IConnectionMultiplexer CreateRedis(IDatabase database)
   {
      var redis = Substitute.For<IConnectionMultiplexer>();
      redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
      return redis;
   }
}
