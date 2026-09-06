using Courier.Application.Services;
using Courier.Application.Validators;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Messages;
using Courier.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Courier.Application.Tests.Services;

public class CourierMessageServiceTests
{
   private readonly IEmailRepository _emailRepository = Substitute.For<IEmailRepository>();
   private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
   private readonly ITemplateRepository _templateRepository = Substitute.For<ITemplateRepository>();
   private readonly IUserPreferenceRepository _userPreferenceRepository = Substitute.For<IUserPreferenceRepository>();
   private readonly CourierMessageService _service;

   public CourierMessageServiceTests()
   {
      _service = new CourierMessageService(
         _emailRepository,
         _notificationRepository,
         _templateRepository,
         _userPreferenceRepository,
         new SimpleEmailTemplateRenderer(),
         new EmailValidator());
   }

   [Fact]
   public async Task QueueAsync_ShouldCreateEmailAndNotification_WhenBothChannelsExist()
   {
      var request = CreateRequest();
      var template = CreateTemplate(request.Module, request.TemplateKey, request.Language, includeEmail: true, includeNotification: true);
      Email? savedEmail = null;
      Notification? savedNotification = null;
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns(template);
      _emailRepository.AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
         .Returns(call =>
         {
            savedEmail = (Email)call[0];
            return savedEmail.Id;
         });
      _notificationRepository.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>())
         .Returns(call =>
         {
            savedNotification = (Notification)call[0];
            return savedNotification.Id;
         });

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      savedEmail.Should().NotBeNull();
      savedEmail!.Subject.Should().Be("Welcome <b>Ana</b>");
      savedEmail.Body.Should().Be("<p>Hello &lt;b&gt;Ana&lt;/b&gt;</p>");
      savedNotification.Should().NotBeNull();
      savedNotification!.Feature.Should().Be(request.Feature);
      savedNotification.Title.Should().Be("New account for <b>Ana</b>");
      savedNotification.Message.Should().Be("Open profile for <b>Ana</b>");
      savedNotification.ActionLink.Should().Be("/users/<b>Ana</b>");
   }

   [Fact]
   public async Task QueueAsync_ShouldCreateOnlyNotification_WhenEmailChannelDoesNotExist()
   {
      var request = CreateRequest() with { Recipient = null };
      var template = CreateTemplate(request.Module, request.TemplateKey, request.Language, includeEmail: false, includeNotification: true);
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _emailRepository.DidNotReceive().AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
      await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task QueueAsync_ShouldCreateOnlyEmail_WhenNotificationChannelDoesNotExist()
   {
      var request = CreateRequest();
      var template = CreateTemplate(request.Module, request.TemplateKey, request.Language, includeEmail: true, includeNotification: false);
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _emailRepository.Received(1).AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
      await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task QueueAsync_ShouldSkipDisabledEmailTemplate_AndStillCreateNotification()
   {
      var request = CreateRequest();
      var template = CreateTemplate(request.Module, request.TemplateKey, request.Language, includeEmail: true, includeNotification: true);
      var preference = UserPreference.CreateDefault(request.UserId);

      preference.DisableEmailTemplatePreference(request.Module, request.TemplateKey);
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns(template);
      _userPreferenceRepository.GetByUserIdAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(preference);

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _emailRepository.DidNotReceive().AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
      await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task QueueAsync_ShouldUseDefaultLanguage_WhenRequestedLanguageDoesNotExist()
   {
      var request = CreateRequest() with { Language = "pt-BR" };
      var template = CreateTemplate(request.Module, request.TemplateKey, "en-US", includeEmail: false, includeNotification: true);
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task QueueAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var request = CreateRequest();
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns((Template?)null);

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is NotFoundError);
   }

   [Fact]
   public async Task QueueAsync_ShouldReturnLanguageError_WhenRequestedAndDefaultTranslationsDoNotExist()
   {
      var request = CreateRequest() with { Language = "pt-BR" };
      var template = CreateTemplate(request.Module, request.TemplateKey, "es", includeEmail: true, includeNotification: false);
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is TemplateLanguageNotFoundError);
   }

   [Fact]
   public async Task QueueAsync_ShouldReturnPlaceholderError_WhenValueIsMissing()
   {
      var request = CreateRequest() with { Values = new Dictionary<string, string>() };
      var template = CreateTemplate(request.Module, request.TemplateKey, request.Language, includeEmail: true, includeNotification: true);
      _templateRepository.GetByModuleAndKeyAsync(request.Module, request.TemplateKey, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.QueueAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is EmailTemplatePlaceholderMissingError);
      await _emailRepository.DidNotReceive().AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
      await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
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
            ["user.name"] = "<b>Ana</b>"
         });
   }

   private static Template CreateTemplate(
      string module,
      string key,
      string language,
      bool includeEmail,
      bool includeNotification)
   {
      var template = Template.Create(
         module,
         key,
         false,
         NotificationSeverity.Information,
         RetentionPolicy.Standard,
         Guid.NewGuid());
      var translation = TemplateTranslation.Create(
         language,
         "Welcome",
         includeEmail
            ? TemplateTranslationEmail.Create("Welcome {{user.name}}", "<p>Hello {{user.name}}</p>")
            : null,
         includeNotification
            ? TemplateTranslationNotification.Create(
               "New account for {{user.name}}",
               "Open profile for {{user.name}}",
               "/users/{{user.name}}")
            : null);
      template.AddTranslation(translation, Guid.NewGuid());

      return template;
   }
}
