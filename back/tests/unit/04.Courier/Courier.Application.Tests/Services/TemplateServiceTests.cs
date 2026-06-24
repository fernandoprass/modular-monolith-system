using Courier.Application.Contracts;
using Courier.Application.Services;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Messages;
using FluentAssertions;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Courier.Application.Tests.Services;

public class TemplateServiceTests
{
   private readonly ITemplateWriteRepository _templateRepository = Substitute.For<ITemplateWriteRepository>();
   private readonly ITemplateValidator _templateValidator = Substitute.For<ITemplateValidator>();
   private readonly IUserContext _userContext = Substitute.For<IUserContext>();
   private readonly TemplateService _service;

   public TemplateServiceTests()
   {
      _service = new TemplateService(_templateRepository, _templateValidator, _userContext);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      _templateRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
         .Returns((Template?)null);

      var result = await _service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is NotFoundError);
   }

   [Fact]
   public async Task CreateAsync_ShouldPersistTemplateWithMetadataAndAudit()
   {
      var userId = Guid.NewGuid();
      var request = CreateRequest();
      _userContext.UserId.Returns(userId);
      _templateRepository.KeyExistsAsync(
         request.Module,
         request.Key,
         null,
         Arg.Any<CancellationToken>()).Returns(false);
      _templateValidator.ValidateCreate(request, keyExists: false).Returns(Result.Success());

      var result = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data!.Module.Should().Be(request.Module);
      result.Data.IsAllowingOptOut.Should().Be(request.IsAllowingOptOut);
      await _templateRepository.Received(1).AddAsync(
         Arg.Is<Template>(template =>
            template.Module == request.Module
            && template.Key == request.Key
            && template.Severity == request.Severity
            && template.CreatedBy == userId),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_ShouldUpdateTemplate_WhenValidationSucceeds()
   {
      var id = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var template = CreateTemplate();
      var request = new TemplateUpdateRequest(
         "courier",
         "updated-template",
         true,
         NotificationSeverity.Critical,
         RetentionPolicy.Extended);
      _userContext.UserId.Returns(userId);
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _templateRepository.KeyExistsAsync(
         request.Module,
         request.Key,
         id,
         Arg.Any<CancellationToken>()).Returns(false);
      _templateValidator.ValidateUpdate(request, templateExists: true, keyExists: false).Returns(Result.Success());

      var result = await _service.UpdateAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _templateRepository.Received(1).UpdateAsync(
         Arg.Is<Template>(updated =>
            updated.Module == request.Module
            && updated.Key == request.Key
            && updated.IsAllowingOptOut
            && updated.Severity == request.Severity
            && updated.UpdatedBy == userId),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AddTranslationAsync_ShouldSanitizeEmailAndStoreBothChannels()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      var request = CreateTranslationRequest() with
      {
         Email = new TemplateTranslationEmailRequest(
            "Welcome user",
            """<p onclick="evil()">Hi</p><script>alert(1)</script><img src="https://example.com/a.png">""")
      };
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _templateValidator.ValidateTranslation(request, templateExists: true).Returns(Result.Success());

      var result = await _service.AddTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      var translation = template.Translations.Should().ContainSingle().Subject;
      translation.Email.Should().NotBeNull();
      translation.Email!.Body.ToLowerInvariant().Should().NotContain("script");
      translation.Email.Body.ToLowerInvariant().Should().NotContain("onclick");
      translation.Email.Body.Should().Contain("https://example.com/a.png");
      translation.Notification.Should().NotBeNull();
   }

   [Fact]
   public async Task AddTranslationAsync_ShouldReturnDuplicateError_WhenLanguageExists()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      var request = CreateTranslationRequest();
      template.AddTranslation(CreateDomainTranslation(), Guid.NewGuid());
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _templateValidator.ValidateTranslation(request, templateExists: true).Returns(Result.Success());

      var result = await _service.AddTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is TemplateTranslationAlreadyExistsError);
   }

   [Fact]
   public async Task UpdateTranslationAsync_ShouldReturnNotFound_WhenLanguageDoesNotExist()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      var request = CreateTranslationRequest();
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _templateValidator.ValidateTranslation(Arg.Any<TemplateTranslationRequest>(), templateExists: true)
         .Returns(Result.Success());

      var result = await _service.UpdateTranslationAsync(
         id,
         "en",
         request,
         TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is TemplateTranslationNotFoundError);
   }

   [Fact]
   public async Task RemoveTranslationAsync_ShouldReturnNotFound_WhenLanguageDoesNotExist()
   {
      var id = Guid.NewGuid();
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(CreateTemplate());

      var result = await _service.RemoveTranslationAsync(id, "en", TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is TemplateTranslationNotFoundError);
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
         "en",
         "User welcome",
         new TemplateTranslationEmailRequest("Welcome user", "<p>Welcome</p>"),
         new TemplateTranslationNotificationRequest("Account created", "Open your profile", "/profile"));
   }

   private static Template CreateTemplate()
   {
      return Template.Create(
         "iam",
         "user-welcome",
         false,
         NotificationSeverity.Information,
         RetentionPolicy.Standard,
         Guid.NewGuid());
   }

   private static Courier.Domain.ValueObjects.TemplateTranslation CreateDomainTranslation()
   {
      return Courier.Domain.ValueObjects.TemplateTranslation.Create(
         "en",
         "User welcome",
         Courier.Domain.ValueObjects.TemplateTranslationEmail.Create("Welcome user", "<p>Welcome</p>"),
         null);
   }
}
