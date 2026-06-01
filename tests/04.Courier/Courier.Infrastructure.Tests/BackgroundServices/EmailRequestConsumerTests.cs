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
using System.Reflection;
using System.Text.Json;

namespace Courier.Infrastructure.Tests.BackgroundServices;

public class EmailRequestConsumerTests
{
   [Fact]
   public async Task ProcessEntryAsync_ShouldQueueEnvelopePayloadAndAcknowledge()
   {
      var database = Substitute.For<IDatabase>();
      var outboxService = Substitute.For<IEmailOutboxService>();
      var consumer = CreateConsumer(database, outboxService);
      var request = CreateRequest();
      var envelope = IntegrationEvent<EmailQueueRequest>.Create(
         CourierConst.Event.Name.EmailRequested,
         CourierConst.Event.Version,
         request);
      var entry = CreateEntry(envelope);
      outboxService.QueueAsync(Arg.Any<EmailQueueRequest>(), Arg.Any<CancellationToken>())
         .Returns(Task.FromResult(Result<Guid>.Success(Guid.NewGuid())));

      await InvokeProcessEntryAsync(consumer, entry);

      await outboxService.Received(1).QueueAsync(
         Arg.Is<EmailQueueRequest>(r =>
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
         CourierConst.Redis.EmailRequestsStream,
         CourierConst.Redis.EmailRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeAndSkipUnsupportedEnvelope()
   {
      var database = Substitute.For<IDatabase>();
      var outboxService = Substitute.For<IEmailOutboxService>();
      var consumer = CreateConsumer(database, outboxService);
      var envelope = IntegrationEvent<EmailQueueRequest>.Create(
         "wrong.event",
         CourierConst.Event.Version,
         CreateRequest());
      var entry = CreateEntry(envelope);

      await InvokeProcessEntryAsync(consumer, entry);

      await outboxService.DidNotReceive().QueueAsync(Arg.Any<EmailQueueRequest>(), Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.EmailRequestsStream,
         CourierConst.Redis.EmailRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeAndSkipUnsupportedVersion()
   {
      var database = Substitute.For<IDatabase>();
      var outboxService = Substitute.For<IEmailOutboxService>();
      var consumer = CreateConsumer(database, outboxService);
      var envelope = IntegrationEvent<EmailQueueRequest>.Create(
         CourierConst.Event.Name.EmailRequested,
         999,
         CreateRequest());
      var entry = CreateEntry(envelope);

      await InvokeProcessEntryAsync(consumer, entry);

      await outboxService.DidNotReceive().QueueAsync(Arg.Any<EmailQueueRequest>(), Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.EmailRequestsStream,
         CourierConst.Redis.EmailRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeNullEnvelope()
   {
      var database = Substitute.For<IDatabase>();
      var outboxService = Substitute.For<IEmailOutboxService>();
      var consumer = CreateConsumer(database, outboxService);
      var entry = new StreamEntry("1-0", [new NameValueEntry(CourierConst.Redis.EventFieldName, "null")]);

      await InvokeProcessEntryAsync(consumer, entry);

      await outboxService.DidNotReceive().QueueAsync(Arg.Any<EmailQueueRequest>(), Arg.Any<CancellationToken>());
      await database.Received(1).StreamAcknowledgeAsync(
         CourierConst.Redis.EmailRequestsStream,
         CourierConst.Redis.EmailRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeInvalidJsonAndLogSystemError()
   {
      var database = Substitute.For<IDatabase>();
      var outboxService = Substitute.For<IEmailOutboxService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var consumer = CreateConsumer(database, outboxService, courierLogger);
      var entry = new StreamEntry("1-0", [new NameValueEntry(CourierConst.Redis.EventFieldName, "{ invalid json")]);

      await InvokeProcessEntryAsync(consumer, entry);

      await outboxService.DidNotReceive().QueueAsync(Arg.Any<EmailQueueRequest>(), Arg.Any<CancellationToken>());
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
         CourierConst.Redis.EmailRequestsStream,
         CourierConst.Redis.EmailRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   [Fact]
   public async Task ProcessEntryAsync_ShouldAcknowledgeAndLogWarning_WhenQueueFails()
   {
      var database = Substitute.For<IDatabase>();
      var outboxService = Substitute.For<IEmailOutboxService>();
      var courierLogger = Substitute.For<ICourierLogger>();
      var consumer = CreateConsumer(database, outboxService, courierLogger);
      var request = CreateRequest();
      var envelope = IntegrationEvent<EmailQueueRequest>.Create(
         CourierConst.Event.Name.EmailRequested,
         CourierConst.Event.Version,
         request);
      var entry = CreateEntry(envelope);
      outboxService.QueueAsync(Arg.Any<EmailQueueRequest>(), Arg.Any<CancellationToken>())
         .Returns(Result<Guid>.Failure(new EmailDeliveryFailedError("queue failed")));

      await InvokeProcessEntryAsync(consumer, entry);

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
         CourierConst.Redis.EmailRequestsStream,
         CourierConst.Redis.EmailRequestConsumerGroup,
         entry.Id,
         Arg.Any<CommandFlags>());
   }

   private static EmailRequestConsumer CreateConsumer(
      IDatabase database,
      IEmailOutboxService outboxService,
      ICourierLogger? courierLogger = null)
   {
      var redis = Substitute.For<IConnectionMultiplexer>();
      redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);

      var services = new ServiceCollection()
         .AddScoped(_ => outboxService)
         .AddScoped(_ => courierLogger ?? Substitute.For<ICourierLogger>())
         .BuildServiceProvider();

      return new EmailRequestConsumer(
         redis,
         services,
         Substitute.For<ILogger<EmailRequestConsumer>>());
   }

   private static StreamEntry CreateEntry(IntegrationEvent<EmailQueueRequest> envelope)
   {
      var json = JsonSerializer.Serialize(envelope);
      return new StreamEntry("1-0", [new NameValueEntry(CourierConst.Redis.EventFieldName, json)]);
   }

   private static async Task InvokeProcessEntryAsync(EmailRequestConsumer consumer, StreamEntry entry)
   {
      var method = typeof(EmailRequestConsumer).GetMethod("ProcessEntryAsync", BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.NotNull(method);

      var task = (Task)method.Invoke(consumer, [entry, TestContext.Current.CancellationToken])!;
      await task;
   }

   private static EmailQueueRequest CreateRequest()
   {
      return new EmailQueueRequest(
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
