using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Messages;
using Courier.Infrastructure.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myce.Response;
using NSubstitute;
using Shared.Domain.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace Courier.Infrastructure.Tests.BackgroundServices;

public class MessageRequestConsumerTests
{
   [Fact]
   public async Task ProcessEntryAsync_ShouldQueueEnvelopePayloadAndAcknowledge()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var consumer = CreateConsumer(database, messageService);
      var request = CreateRequest();
      var envelope = IntegrationEvent<CourierMessageRequest>.Create(
         CourierConst.Event.Name.MessageRequested,
         CourierConst.Event.Version,
         request);
      var entry = CreateEntry(envelope);
      messageService.QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>())
         .Returns(Task.FromResult(Result.Success()));

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await messageService.Received(1).QueueAsync(
         Arg.Is<CourierMessageRequest>(r =>
            r.OrganizationId == request.OrganizationId &&
            r.UserId == request.UserId &&
            r.Module == request.Module &&
            r.Feature == request.Feature &&
            r.TemplateKey == request.TemplateKey &&
            r.Language == request.Language &&
            r.Recipient == request.Recipient &&
            r.Values!["user.name"] == "Test"),
         Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeAndSkipUnsupportedEnvelope()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var consumer = CreateConsumer(database, messageService);
      var envelope = IntegrationEvent<CourierMessageRequest>.Create(
         "wrong.event",
         CourierConst.Event.Version,
         CreateRequest());
      var entry = CreateEntry(envelope);

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await messageService.DidNotReceive().QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeAndSkipUnsupportedVersion()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var consumer = CreateConsumer(database, messageService);
      var envelope = IntegrationEvent<CourierMessageRequest>.Create(
         CourierConst.Event.Name.MessageRequested,
         999,
         CreateRequest());
      var entry = CreateEntry(envelope);

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await messageService.DidNotReceive().QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledge_WhenEventFieldIsMissing()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var consumer = CreateConsumer(database, messageService);
      var entry = new StreamEntry("1-0", [new NameValueEntry("wrong-field", "{}")]);

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await messageService.DidNotReceive().QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeNullEnvelope()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var consumer = CreateConsumer(database, messageService);
      var entry = new StreamEntry("1-0", [new NameValueEntry(CourierConst.Redis.EventFieldName, "null")]);

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await messageService.DidNotReceive().QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeAndLogWarning_WhenPayloadIsMissing()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var consumer = CreateConsumer(database, messageService, courierLogger);
      var json = $$"""
         {
           "eventId": "{{Guid.NewGuid()}}",
           "eventName": "{{CourierConst.Event.Name.MessageRequested}}",
           "version": {{CourierConst.Event.Version}},
           "correlationId": null,
           "createdAt": "{{DateTime.UtcNow:O}}",
           "payload": null
         }
         """;
      var entry = new StreamEntry("1-0", [new NameValueEntry(CourierConst.Redis.EventFieldName, json)]);

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await messageService.DidNotReceive().QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>());
      await courierLogger.Received(1).LogSystemAsync(
         Shared.Domain.Enums.SystemLogLevel.Warning,
         Shared.Domain.Enums.SystemLogStatus.Failure,
         Arg.Any<string>(),
         null,
         null,
         null,
         Arg.Is<Dictionary<string, object>>(p => p["streamId"].ToString() == entry.Id.ToString()),
         Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeInvalidJsonAndLogSystemError()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var consumer = CreateConsumer(database, messageService, courierLogger);
      var entry = new StreamEntry("1-0", [new NameValueEntry(CourierConst.Redis.EventFieldName, "{ invalid json")]);

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await messageService.DidNotReceive().QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>());
      await courierLogger.Received(1).LogSystemAsync(
         Shared.Domain.Enums.SystemLogLevel.Error,
         Shared.Domain.Enums.SystemLogStatus.Failure,
         Arg.Any<string>(),
         Arg.Any<JsonException>(),
         null,
         null,
         null,
         Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldLogSystemErrorAndNotAcknowledge_WhenQueueThrows()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var consumer = CreateConsumer(database, messageService, courierLogger);
      var entry = CreateEntry(IntegrationEvent<CourierMessageRequest>.Create(
         CourierConst.Event.Name.MessageRequested,
         CourierConst.Event.Version,
         CreateRequest()));
      messageService.QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>())
         .Returns<Task<Result>>(_ => throw new InvalidOperationException("queue error"));

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await courierLogger.Received(1).LogSystemAsync(
         Shared.Domain.Enums.SystemLogLevel.Error,
         Shared.Domain.Enums.SystemLogStatus.Failure,
         Arg.Any<string>(),
         Arg.Any<InvalidOperationException>(),
         null,
         null,
         null,
         Arg.Any<CancellationToken>());
      await database.DidNotReceive().StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeAndLogWarning_WhenQueueFails()
   {
      var database = Substitute.For<IDatabase>();
      var messageService = Substitute.For<ICourierMessageService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var consumer = CreateConsumer(database, messageService, courierLogger);
      var request = CreateRequest();
      var envelope = IntegrationEvent<CourierMessageRequest>.Create(
         CourierConst.Event.Name.MessageRequested,
         CourierConst.Event.Version,
         request);
      var entry = CreateEntry(envelope);
      messageService.QueueAsync(Arg.Any<CourierMessageRequest>(), Arg.Any<CancellationToken>())
         .Returns(Result.Failure(new EmailDeliveryFailedError("queue failed")));

      await consumer.ProcessEntryAsync(entry, TestContext.Current.CancellationToken);

      await courierLogger.Received(1).LogSystemAsync(
         Shared.Domain.Enums.SystemLogLevel.Warning,
         Shared.Domain.Enums.SystemLogStatus.Failure,
         Arg.Any<string>(),
         null,
         request.OrganizationId,
         request.UserId,
         Arg.Any<Dictionary<string, object>>(),
         Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.MessageRequestsStream,
         CourierConst.Redis.MessageRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   private static MessageRequestConsumer CreateConsumer(
      IDatabase database,
      ICourierMessageService messageService,
      ICourierLogger? courierLogger = null)
   {
      var redis = Substitute.For<IConnectionMultiplexer>();
      redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);

      var services = new ServiceCollection()
         .AddScoped(_ => messageService)
         .AddScoped(_ => courierLogger ?? Substitute.For<ICourierLogger>())
         .BuildServiceProvider();

      return new MessageRequestConsumer(
         redis,
         services,
         Substitute.For<ILogger<MessageRequestConsumer>>());
   }

   private static StreamEntry CreateEntry(IntegrationEvent<CourierMessageRequest> envelope)
   {
      var json = JsonSerializer.Serialize(envelope);
      return new StreamEntry("1-0", [new NameValueEntry(CourierConst.Redis.EventFieldName, json)]);
   }

   private static CourierMessageRequest CreateRequest()
   {
      return new CourierMessageRequest(
         Guid.NewGuid(),
         Guid.NewGuid(),
         "iam",
         "users",
         "welcome-email",
         "en",
         "person@example.com",
         new Dictionary<string, string>
         {
            ["user.name"] = "Test"
         });
   }
}
