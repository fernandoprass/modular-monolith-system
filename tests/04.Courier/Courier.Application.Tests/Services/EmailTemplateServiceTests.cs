using Courier.Application.Contracts;
using Courier.Application.Services;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Messages;
using FluentAssertions;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Messages;

namespace Courier.Application.Tests.Services;

public class EmailTemplateServiceTests
{
   private readonly IEmailTemplateWriteRepository _emailTemplateRepository = Substitute.For<IEmailTemplateWriteRepository>();
   private readonly IEmailTemplateValidator _emailTemplateValidator = Substitute.For<IEmailTemplateValidator>();
   private readonly IUserContext _userContext = Substitute.For<IUserContext>();
   private readonly EmailTemplateService _service;

   public EmailTemplateServiceTests()
   {
      _service = new EmailTemplateService(_emailTemplateRepository, _emailTemplateValidator, _userContext);
   }

   [Fact]
   public async Task GetAsync_ShouldReturnRepositoryResult()
   {
      var request = new EmailTemplateSearchRequest("welcome", 1, 25);
      var page = new PagedResultDto<EmailTemplateDto>([], 1, 25, 0, 0);
      _emailTemplateValidator.ValidateSearch(request).Returns(Result.Success());
      _emailTemplateRepository.GetAsync(request, Arg.Any<CancellationToken>()).Returns(page);

      var result = await _service.GetAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().Be(page);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var id = Guid.NewGuid();
      _emailTemplateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((EmailTemplate?)null);

      var result = await _service.GetByIdAsync(id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is NotFoundError);
   }

   [Fact]
   public async Task CreateAsync_ShouldPersistTemplateWithLoggedUserId()
   {
      var userId = Guid.NewGuid();
      var request = CreateRequest();
      _userContext.UserId.Returns(userId);
      _emailTemplateRepository.KeyExistsAsync(request.Key, null, Arg.Any<CancellationToken>()).Returns(false);
      _emailTemplateValidator.ValidateCreate(request, keyExists: false).Returns(Result.Success());

      var result = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data!.CreatedBy.Should().Be(userId);
      await _emailTemplateRepository.Received(1).AddAsync(
         Arg.Is<EmailTemplate>(t =>
            t.Key == request.Key &&
            t.Name == request.Name &&
            t.RetentionPolicy == request.RetentionPolicy &&
            t.CreatedBy == userId),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_ShouldUpdateTemplate_WhenValidationSucceeds()
   {
      var id = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var template = CreateTemplate();
      var request = new EmailTemplateUpdateRequest("updated-email", "Updated", EmailRetentionPolicy.Extended);
      _userContext.UserId.Returns(userId);
      _emailTemplateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _emailTemplateRepository.KeyExistsAsync(request.Key, id, Arg.Any<CancellationToken>()).Returns(false);
      _emailTemplateValidator.ValidateUpdate(request, templateExists: true, keyExists: false).Returns(Result.Success());

      var result = await _service.UpdateAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _emailTemplateRepository.Received(1).UpdateAsync(
         Arg.Is<EmailTemplate>(t =>
            t.Key == request.Key &&
            t.Name == request.Name &&
            t.RetentionPolicy == request.RetentionPolicy &&
            t.UpdatedBy == userId),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var id = Guid.NewGuid();
      _emailTemplateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((EmailTemplate?)null);

      var result = await _service.DeleteAsync(id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is NotFoundError);
      await _emailTemplateRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AddTranslationAsync_ShouldSanitizeDangerousHtml()
   {
      var id = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var template = CreateTemplate();
      var request = new EmailTemplateTranslationRequest(
         "en",
         "Welcome",
         """<p onclick="evil()">Hi</p><script>alert(1)</script><a href="javascript:alert(1)">Link</a><img src="https://example.com/a.png">""");
      _userContext.UserId.Returns(userId);
      _emailTemplateValidator.ValidateTranslation(request).Returns(Result.Success());
      _emailTemplateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.AddTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _emailTemplateRepository.Received(1).UpdateAsync(
         Arg.Is<EmailTemplate>(t => BodyIsSanitized(t)),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AddTranslationAsync_ShouldReturnDuplicateError_WhenLanguageExists()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      template.AddTranslation("en", "Welcome", "Body", Guid.NewGuid());
      var request = new EmailTemplateTranslationRequest("en", "Welcome", "Body");
      _emailTemplateValidator.ValidateTranslation(request).Returns(Result.Success());
      _emailTemplateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.AddTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is EmailTemplateTranslationAlreadyExistsError);
      await _emailTemplateRepository.DidNotReceive().UpdateAsync(Arg.Any<EmailTemplate>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateTranslationAsync_ShouldReturnNotFound_WhenLanguageDoesNotExist()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      var request = new EmailTemplateTranslationRequest("en", "Welcome", "Body");
      _emailTemplateValidator.ValidateTranslation(request).Returns(Result.Success());
      _emailTemplateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.UpdateTranslationAsync(id, "en", request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is EmailTemplateTranslationNotFoundError);
      await _emailTemplateRepository.DidNotReceive().UpdateAsync(Arg.Any<EmailTemplate>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task RemoveTranslationAsync_ShouldReturnNotFound_WhenLanguageDoesNotExist()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      _emailTemplateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.RemoveTranslationAsync(id, "en", TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is EmailTemplateTranslationNotFoundError);
      await _emailTemplateRepository.DidNotReceive().UpdateAsync(Arg.Any<EmailTemplate>(), Arg.Any<CancellationToken>());
   }

   private static EmailTemplateCreateRequest CreateRequest()
   {
      return new EmailTemplateCreateRequest("welcome-email", "Welcome", EmailRetentionPolicy.Standard);
   }

   private static EmailTemplate CreateTemplate()
   {
      return EmailTemplate.Create("welcome-email", "Welcome", EmailRetentionPolicy.Standard, Guid.NewGuid());
   }

   private static bool BodyIsSanitized(EmailTemplate template)
   {
      var body = template.Translations.Single().Body;

      return !body.Contains("<script>", StringComparison.OrdinalIgnoreCase)
         && !body.Contains("onclick", StringComparison.OrdinalIgnoreCase)
         && !body.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
         && body.Contains("https://example.com/a.png", StringComparison.OrdinalIgnoreCase);
   }
}
