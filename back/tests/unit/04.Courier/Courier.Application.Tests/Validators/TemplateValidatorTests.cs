using Courier.Application.Validators;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Enums;
using Courier.Domain.Messages;
using FluentAssertions;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Courier.Application.Tests.Validators;

public class TemplateValidatorTests
{
   private readonly TemplateValidator _validator = new();

   [Fact]
   public void ValidateCreate_ShouldSucceed_WhenRequestIsValid()
   {
      var result = _validator.ValidateCreate(CreateRequest(), keyExists: false);

      result.HasError.Should().BeFalse();
   }

   [Fact]
   public void ValidateCreate_ShouldReturnDuplicateError_WhenModuleKeyExists()
   {
      var result = _validator.ValidateCreate(CreateRequest(), keyExists: true);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message.Code.Equals(CourierTranslatedMessagesProvider.TemplateDuplicateKeyError));
   }

   [Fact]
   public void ValidateUpdate_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var request = new TemplateUpdateRequest(
         "iam",
         "user-welcome",
         false,
         NotificationSeverity.Information,
         RetentionPolicy.Standard);

      var result = _validator.ValidateUpdate(request, templateExists: false, keyExists: false);

      result.HasError.Should().BeTrue();
      result.Messages.Should().Contain(message => message.Code.Equals(SharedTranslatedMessagesProvider.NotFoundDetailedError));
      result.Messages.Should().Contain(message => message.Variables.First().Value.Equals(CourierConst.Entity.Template));
   }

   [Fact]
   public void ValidateTranslation_ShouldSucceed_WithBothChannels()
   {
      var result = _validator.ValidateTranslation(CreateTranslationRequest(), templateExists: true);

      result.HasError.Should().BeFalse();
   }

   [Fact]
   public void ValidateTranslation_ShouldRejectUnsupportedLanguage()
   {
      var request = CreateTranslationRequest() with { Language = "xx-ZZ" };

      var result = _validator.ValidateTranslation(request, templateExists: true);

      result.HasError.Should().BeTrue();
      result.Messages.Should().Contain(message => message.Code.Equals(SharedTranslatedMessagesProvider.InvalidLanguageError));
   }

   [Fact]
   public void ValidateTranslation_ShouldRequireAtLeastOneChannel()
   {
      var request = CreateTranslationRequest() with { Email = null, Notification = null };

      var result = _validator.ValidateTranslation(request, templateExists: true);

      result.HasError.Should().BeTrue();
      result.Messages.Should().Contain(message => message.Code.Equals(CourierTranslatedMessagesProvider.TemplateChannelRequiredError));
   }

   [Fact]
   public void ValidateTranslation_ShouldValidateEmailFields()
   {
      var request = CreateTranslationRequest() with
      {
         Email = new TemplateTranslationEmailRequest("short", string.Empty)
      };

      var result = _validator.ValidateTranslation(request, templateExists: true);

      result.HasError.Should().BeTrue();
   }

   [Fact]
   public void ValidateTranslation_ShouldValidateNotificationFields()
   {
      var request = CreateTranslationRequest() with
      {
         Notification = new TemplateTranslationNotificationRequest(string.Empty, string.Empty, null)
      };

      var result = _validator.ValidateTranslation(request, templateExists: true);

      result.HasError.Should().BeTrue();
   }

   private static TemplateCreateRequest CreateRequest()
   {
      return new TemplateCreateRequest(
         "iam",
         "user-welcome",
         false,
         NotificationSeverity.Information,
         RetentionPolicy.Standard);
   }

   private static TemplateTranslationRequest CreateTranslationRequest()
   {
      return new TemplateTranslationRequest(
         "en-US",
         "User welcome",
         new TemplateTranslationEmailRequest("Welcome user", "<p>Welcome</p>"),
         new TemplateTranslationNotificationRequest("Account created", "Open your profile", "/profile"));
   }
}
