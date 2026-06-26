using Courier.Application.Contracts;
using Courier.Application.Services;
using Courier.Domain;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Messages;
using FluentAssertions;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Enums;

namespace Courier.Application.Tests.Services;

public class EmailOutboxServiceTests
{
   private readonly IEmailRepository _emailRepository = Substitute.For<IEmailRepository>();
   private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
   private readonly IParameterService _parameterService = Substitute.For<IParameterService>();
   private readonly ICourierLogger _courierLogger = Substitute.For<ICourierLogger>();
   private readonly EmailOutboxService _service;

   public EmailOutboxServiceTests()
   {
      _service = new EmailOutboxService(
         _emailRepository,
         _emailSender,
         _parameterService,
         _courierLogger);
   }

   [Fact]
   public async Task ProcessNextPendingAsync_ShouldReturnFalse_WhenNoEmailIsPending()
   {
      _emailRepository.ClaimNextPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns((Email?)null);

      var result = await _service.ProcessNextPendingAsync(TestContext.Current.CancellationToken);

      result.Should().BeFalse();
      await _emailSender.DidNotReceive().SendAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task ProcessNextPendingAsync_ShouldMarkEmailAsSentAndLogAudit()
   {
      var email = CreateEmail();
      _emailRepository.ClaimNextPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(email);
      _emailSender.SendAsync(email, Arg.Any<CancellationToken>()).Returns(Result.Success());

      var result = await _service.ProcessNextPendingAsync(TestContext.Current.CancellationToken);

      result.Should().BeTrue();
      email.Status.Should().Be(EmailStatus.Sent);
      email.SentAt.Should().NotBeNull();
      await _emailRepository.Received(1).UpdateAsync(email, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task ProcessNextPendingAsync_ShouldRecordFailureAndLog_WhenSenderFails()
   {
      var email = CreateEmail();
      _emailRepository.ClaimNextPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(email);
      _emailSender.SendAsync(email, Arg.Any<CancellationToken>())
         .Returns(Result.Failure(new EmailDeliveryFailedError("send failed")));
      _parameterService.GetIntAsync(CourierParam.EmailDelivery.MaxRetries, Arg.Any<CancellationToken>()).Returns(2);

      var result = await _service.ProcessNextPendingAsync(TestContext.Current.CancellationToken);

      result.Should().BeTrue();
      email.Status.Should().Be(EmailStatus.Pending);
      email.RetryCount.Should().Be(1);
      email.Attempts.Should().ContainSingle();
      await _emailRepository.Received(1).UpdateAsync(email, Arg.Any<CancellationToken>());

      await _courierLogger.Received(1).LogSystemAsync(
         SystemLogLevel.Error,
         SystemLogStatus.Failure,
         Arg.Any<string>(),
         null,
         email.OrganizationId,
         email.UserId,
         Arg.Any<Dictionary<string, object>>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task ProcessNextPendingAsync_ShouldUseDefaultMaxRetries_WhenParameterLoadFails()
   {
      var email = CreateEmail();
      _emailRepository.ClaimNextPendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(email);
      _emailSender.SendAsync(email, Arg.Any<CancellationToken>())
         .Returns(Result.Failure(new EmailDeliveryFailedError("send failed")));
      _parameterService.GetIntAsync(CourierParam.EmailDelivery.MaxRetries, Arg.Any<CancellationToken>())
         .Returns(Task.FromException<int>(new InvalidOperationException("missing parameter")));

      var result = await _service.ProcessNextPendingAsync(TestContext.Current.CancellationToken);

      result.Should().BeTrue();
      email.Status.Should().Be(EmailStatus.Pending);
      email.RetryCount.Should().Be(1);
      await _courierLogger.Received(1).LogSystemAsync(
         SystemLogLevel.Warning,
         SystemLogStatus.Failure,
         Arg.Any<string>(),
         Arg.Any<InvalidOperationException>(),
         email.OrganizationId,
         email.UserId,
         Arg.Any<Dictionary<string, object>>(),
         Arg.Any<CancellationToken>());
   }

   private static Email CreateEmail()
   {
      return Email.Create(
         Guid.NewGuid(),
         Guid.NewGuid(),
         "iam",
         "users",
         "welcome-email",
         "person@example.com",
         "Subject",
         "Body",
         false,
         RetentionPolicy.Standard);
   }
}
