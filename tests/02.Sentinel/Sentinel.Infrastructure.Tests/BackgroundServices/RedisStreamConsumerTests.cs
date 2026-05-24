using Microsoft.Extensions.Logging;
using NSubstitute;
using Sentinel.Domain;
using Sentinel.Infrastructure.BackgroundServices;
using Shared.Domain;
using Shared.Domain.Events;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json;

namespace Sentinel.Infrastructure.Tests.BackgroundServices;

public class RedisStreamConsumerTests
{
   [Fact]
   public async Task ExecuteAsync_ShouldProcessAndAcknowledgeValidEvent()
   {
      var database = Substitute.For<IDatabase>();
      var redis = CreateRedis(database);
      var consumer = new TestRedisStreamConsumer(redis);
      var payload = JsonSerializer.Serialize(IntegrationEvent<TestEvent>.Create(
         TestRedisStreamConsumer.TestEventName,
         SharedConst.Event.Version,
         new TestEvent("created")));
      var entry = new StreamEntry("1-0", [new NameValueEntry(SentinelConst.Redis.EventFieldName, payload)]);

      await InvokeProcessEntryAsync(consumer, entry);

      Assert.Single(consumer.ProcessedEvents);
      Assert.Equal("created", consumer.ProcessedEvents[0].Name);
      await database.Received(1).StreamAcknowledgeAsync("test-stream", "test-group", "1-0", Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ExecuteAsync_ShouldAcknowledgeMissingEventPayload()
   {
      var database = Substitute.For<IDatabase>();
      var redis = CreateRedis(database);
      var consumer = new TestRedisStreamConsumer(redis);
      var entry = new StreamEntry("1-0", []);

      await InvokeProcessEntryAsync(consumer, entry);

      Assert.Empty(consumer.ProcessedEvents);
      await database.Received(1).StreamAcknowledgeAsync("test-stream", "test-group", "1-0", Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ExecuteAsync_ShouldNotAcknowledge_WhenProcessingFails()
   {
      var database = Substitute.For<IDatabase>();
      var redis = CreateRedis(database);
      var consumer = new TestRedisStreamConsumer(redis, shouldThrow: true);
      var payload = JsonSerializer.Serialize(IntegrationEvent<TestEvent>.Create(
         TestRedisStreamConsumer.TestEventName,
         SharedConst.Event.Version,
         new TestEvent("created")));
      var entry = new StreamEntry("1-0", [new NameValueEntry(SentinelConst.Redis.EventFieldName, payload)]);

      await InvokeProcessEntryAsync(consumer, entry);

      await database.DidNotReceive().StreamAcknowledgeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>());
   }

   private static IConnectionMultiplexer CreateRedis(IDatabase database)
   {
      var redis = Substitute.For<IConnectionMultiplexer>();
      redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
      return redis;
   }

   private static async Task InvokeProcessEntryAsync(TestRedisStreamConsumer consumer, StreamEntry entry)
   {
      var method = typeof(RedisStreamConsumer<TestEvent>).GetMethod("ProcessEntryAsync", BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.NotNull(method);

      var task = (Task)method.Invoke(consumer, [entry, TestContext.Current.CancellationToken])!;
      await task;
   }

   private record TestEvent(string Name);

   private class TestRedisStreamConsumer(
      IConnectionMultiplexer redis,
      Action? onProcess = null,
      bool shouldThrow = false) : RedisStreamConsumer<TestEvent>(redis, Substitute.For<ILogger>())
   {
      public const string TestEventName = "test.event.created";
      public List<TestEvent> ProcessedEvents { get; } = [];

      protected override string StreamName => "test-stream";
      protected override string ConsumerGroup => "test-group";
      protected override string ConsumerNamePrefix => "test-consumer";
      protected override string ConsumerDisplayName => "Test consumer";
      protected override string ProcessingErrorMessage => "Error processing test event";
      protected override string ExpectedEventName => TestEventName;

      protected override Task ProcessEventAsync(TestEvent eventData, CancellationToken cancellationToken)
      {
         ProcessedEvents.Add(eventData);
         onProcess?.Invoke();

         if (shouldThrow)
         {
            throw new InvalidOperationException("Processing failed");
         }

         return Task.CompletedTask;
      }
   }
}
