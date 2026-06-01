using Courier.Domain;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using FluentAssertions;

namespace Courier.Domain.Tests.Entities;

public class EmailTests
{
   [Fact]
   public void Create_ShouldNormalizeValuesAndSetPendingStatus()
   {
      var organizationId = Guid.NewGuid();
      var userId = Guid.NewGuid();

      var email = Email.Create(
         organizationId,
         userId,
         " IAM ",
         " Users ",
         " welcome-email ",
         " PERSON@Example.COM ",
         " Subject ",
         "Body",
         true,
         RetentionPolicy.Standard);

      email.OrganizationId.Should().Be(organizationId);
      email.UserId.Should().Be(userId);
      email.Module.Should().Be("IAM");
      email.Feature.Should().Be("Users");
      email.TemplateKey.Should().Be("welcome-email");
      email.Recipient.Should().Be("person@example.com");
      email.Subject.Should().Be("Subject");
      email.IsHtml.Should().BeTrue();
      email.Status.Should().Be(EmailStatus.Pending);
      email.RetryCount.Should().Be(0);
      email.NextAttemptAt.Should().NotBeNull();
      email.SentAt.Should().BeNull();
   }

   [Theory]
   [InlineData(RetentionPolicy.Operational, CourierConst.EmailRetentionPoliciesTimeSpans.Operational)]
   [InlineData(RetentionPolicy.Standard, CourierConst.EmailRetentionPoliciesTimeSpans.Standard)]
   [InlineData(RetentionPolicy.Extended, CourierConst.EmailRetentionPoliciesTimeSpans.Extended)]
   [InlineData(RetentionPolicy.Compliance, CourierConst.EmailRetentionPoliciesTimeSpans.Compliance)]
   [InlineData(RetentionPolicy.LongTerm, CourierConst.EmailRetentionPoliciesTimeSpans.LongTerm)]
   public void Create_ShouldSetExpirationFromRetentionPolicy(RetentionPolicy retentionPolicy, int expectedDays)
   {
      var email = CreateEmail(retentionPolicy);

      var actualDays = (email.ExpiresAt.Date - email.CreatedAt.Date).Days;

      actualDays.Should().Be(expectedDays);
   }

   [Fact]
   public void MarkAsProcessing_ShouldSetProcessingStatus()
   {
      var email = CreateEmail();

      email.MarkAsProcessing();

      email.Status.Should().Be(EmailStatus.Processing);
   }

   [Fact]
   public void MarkAsProcessing_ShouldNotChangeSentEmail()
   {
      var email = CreateEmail();
      email.MarkAsSent();

      email.MarkAsProcessing();

      email.Status.Should().Be(EmailStatus.Sent);
   }

   [Fact]
   public void MarkAsSent_ShouldSetSentStatusAndClearNextAttempt()
   {
      var email = CreateEmail();

      email.MarkAsSent();

      email.Status.Should().Be(EmailStatus.Sent);
      email.SentAt.Should().NotBeNull();
      email.NextAttemptAt.Should().BeNull();
   }

   [Fact]
   public void RecordFailure_ShouldKeepPending_WhenRetriesRemain()
   {
      var email = CreateEmail();

      email.RecordFailure("send failed", "stack", maxRetriesThreshold: 2);

      email.Status.Should().Be(EmailStatus.Pending);
      email.RetryCount.Should().Be(1);
      email.NextAttemptAt.Should().NotBeNull();
      email.Attempts.Should().ContainSingle(attempt =>
         attempt.ErrorMessage == "send failed" &&
         attempt.StackTrace == "stack");
   }

   [Fact]
   public void RecordFailure_ShouldSetFailed_WhenMaxRetriesReached()
   {
      var email = CreateEmail();

      email.RecordFailure("send failed", null, maxRetriesThreshold: 1);

      email.Status.Should().Be(EmailStatus.Failed);
      email.RetryCount.Should().Be(1);
      email.NextAttemptAt.Should().BeNull();
      email.Attempts.Should().ContainSingle();
   }

   private static Email CreateEmail(RetentionPolicy retentionPolicy = RetentionPolicy.Standard)
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
         retentionPolicy);
   }
}
