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
using Shared.Domain.DTOs.Responses;
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
   public async Task GetAsync_ShouldReturnRepositoryResult()
   {
      var request = new TemplateSearchRequest("welcome", null, TemplateType.Email, 1, 25);
      var page = new PagedResultDto<TemplateLiteDto>([], 1, 25, 0, 0);
      _templateValidator.ValidateSearch(request).Returns(Result.Success());
      _templateRepository.GetAsync(request, Arg.Any<CancellationToken>()).Returns(page);

      var result = await _service.GetAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().Be(page);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var id = Guid.NewGuid();
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Template?)null);

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
      _templateRepository.KeyExistsAsync(request.Key, null, Arg.Any<CancellationToken>()).Returns(false);
      _templateValidator.ValidateCreate(request, keyExists: false).Returns(Result.Success());

      var result = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data!.CreatedBy.Should().Be(userId);
      await _templateRepository.Received(1).AddAsync(
         Arg.Is<Template>(t =>
            t.Key == request.Key &&
            t.Name == request.Name &&
            t.Type == request.Type &&
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
      var request = new TemplateUpdateRequest("updated-email", "Updated", TemplateType.Email, RetentionPolicy.Extended);
      _userContext.UserId.Returns(userId);
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _templateRepository.KeyExistsAsync(request.Key, id, Arg.Any<CancellationToken>()).Returns(false);
      _templateValidator.ValidateUpdate(request, templateExists: true, keyExists: false).Returns(Result.Success());

      var result = await _service.UpdateAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _templateRepository.Received(1).UpdateAsync(
         Arg.Is<Template>(t =>
            t.Key == request.Key &&
            t.Name == request.Name &&
            t.Type == request.Type &&
            t.RetentionPolicy == request.RetentionPolicy &&
            t.UpdatedBy == userId),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var id = Guid.NewGuid();
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Template?)null);

      var result = await _service.DeleteAsync(id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is NotFoundError);
      await _templateRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AddEmailTranslationAsync_ShouldSanitizeDangerousHtml()
   {
      var id = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var template = CreateTemplate();
      var request = new TemplateEmailTranslationRequest(
         "en",
         "Welcome",
         """<p onclick="evil()">Hi</p><script>alert(1)</script><a href="javascript:alert(1)">Link</a><img src="https://example.com/a.png">""");
      _userContext.UserId.Returns(userId);
      _templateValidator.ValidateEmailTranslation(request, templateExists: true, isEmailTemplate: true).Returns(Result.Success());
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.AddEmailTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _templateRepository.Received(1).UpdateAsync(
         Arg.Is<Template>(t => BodyIsSanitized(t)),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AddEmailTranslationAsync_ShouldReturnDuplicateError_WhenLanguageExists()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      template.AddEmailTranslation("en", "Welcome", "Body", Guid.NewGuid());
      var request = new TemplateEmailTranslationRequest("en", "Welcome", "Body");
      _templateValidator.ValidateEmailTranslation(request, templateExists: true, isEmailTemplate: true).Returns(Result.Success());
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.AddEmailTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is EmailTemplateTranslationAlreadyExistsError);
      await _templateRepository.DidNotReceive().UpdateAsync(Arg.Any<Template>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AddEmailTranslationAsync_ShouldReturnValidationError_WhenTemplateDoesNotExist()
   {
      var id = Guid.NewGuid();
      var request = new TemplateEmailTranslationRequest("en", "Valid subject", "Body");
      var validation = Result.Failure(new NotFoundError("Template"));
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Template?)null);
      _templateValidator
         .ValidateEmailTranslation(request, templateExists: false, isEmailTemplate: false)
         .Returns(validation);

      var result = await _service.AddEmailTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().BeEquivalentTo(validation.Messages);
      await _templateRepository.DidNotReceive().UpdateAsync(Arg.Any<Template>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AddEmailTranslationAsync_ShouldReturnValidationError_WhenTemplateIsNotEmail()
   {
      var id = Guid.NewGuid();
      var template = Template.Create("template", "Template", TemplateType.Notification, RetentionPolicy.Standard, Guid.NewGuid());
      var request = new TemplateEmailTranslationRequest("en", "Valid subject", "Body");
      var validation = Result.Failure(new TemplateTypeMismatchError(TemplateType.Email.ToString()));
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _templateValidator
         .ValidateEmailTranslation(request, templateExists: true, isEmailTemplate: false)
         .Returns(validation);

      var result = await _service.AddEmailTranslationAsync(id, request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().BeEquivalentTo(validation.Messages);
      await _templateRepository.DidNotReceive().UpdateAsync(Arg.Any<Template>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateEmailTranslationAsync_ShouldReturnNotFound_WhenLanguageDoesNotExist()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      var request = new TemplateEmailTranslationRequest("en", "Welcome", "Body");
      _templateValidator.ValidateEmailTranslation(request, templateExists: true, isEmailTemplate: true).Returns(Result.Success());
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.UpdateEmailTranslationAsync(id, "en", request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is EmailTemplateTranslationNotFoundError);
      await _templateRepository.DidNotReceive().UpdateAsync(Arg.Any<Template>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateEmailTranslationAsync_ShouldReturnValidationError_WhenTemplateDoesNotExist()
   {
      var id = Guid.NewGuid();
      var request = new TemplateEmailTranslationRequest("en", "Valid subject", "Body");
      var validation = Result.Failure(new NotFoundError("Template"));
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Template?)null);
      _templateValidator
         .ValidateEmailTranslation(request, templateExists: false, isEmailTemplate: false)
         .Returns(validation);

      var result = await _service.UpdateEmailTranslationAsync(id, "en", request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().BeEquivalentTo(validation.Messages);
      await _templateRepository.DidNotReceive().UpdateAsync(Arg.Any<Template>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateEmailTranslationAsync_ShouldReturnValidationError_WhenTemplateIsNotEmail()
   {
      var id = Guid.NewGuid();
      var template = Template.Create("template", "Template", TemplateType.Notification, RetentionPolicy.Standard, Guid.NewGuid());
      var request = new TemplateEmailTranslationRequest("en", "Valid subject", "Body");
      var validation = Result.Failure(new TemplateTypeMismatchError(TemplateType.Email.ToString()));
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);
      _templateValidator
         .ValidateEmailTranslation(request, templateExists: true, isEmailTemplate: false)
         .Returns(validation);

      var result = await _service.UpdateEmailTranslationAsync(id, "en", request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().BeEquivalentTo(validation.Messages);
      await _templateRepository.DidNotReceive().UpdateAsync(Arg.Any<Template>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task RemoveTranslationAsync_ShouldReturnNotFound_WhenLanguageDoesNotExist()
   {
      var id = Guid.NewGuid();
      var template = CreateTemplate();
      _templateRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(template);

      var result = await _service.RemoveTranslationAsync(id, "en", TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is EmailTemplateTranslationNotFoundError);
      await _templateRepository.DidNotReceive().UpdateAsync(Arg.Any<Template>(), Arg.Any<CancellationToken>());
   }

   private static TemplateCreateRequest CreateRequest()
   {
      return new TemplateCreateRequest("welcome-email", "Welcome", TemplateType.Email, RetentionPolicy.Standard);
   }

   private static Template CreateTemplate()
   {
      return Template.Create("welcome-email", "Welcome", TemplateType.Email, RetentionPolicy.Standard, Guid.NewGuid());
   }

   private static bool BodyIsSanitized(Template template)
   {
      var body = template.EmailTranslations.Single().Body;

      return !body.Contains("<script>", StringComparison.OrdinalIgnoreCase)
         && !body.Contains("onclick", StringComparison.OrdinalIgnoreCase)
         && !body.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
         && body.Contains("https://example.com/a.png", StringComparison.OrdinalIgnoreCase);
   }
}
