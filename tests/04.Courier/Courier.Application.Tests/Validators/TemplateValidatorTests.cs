using Courier.Application.Validators;
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
   public void ValidateCreate_ShouldReturnSuccess_WhenRequestIsValid()
   {
      var request = CreateRequest();

      var result = _validator.ValidateCreate(request, keyExists: false);

      result.HasError.Should().BeFalse();
   }

   [Fact]
   public void ValidateCreate_ShouldReturnDuplicateKeyError_WhenKeyExists()
   {
      var request = CreateRequest();

      var result = _validator.ValidateCreate(request, keyExists: true);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is TemplateDuplicateKeyError);
   }

   [Fact]
   public void ValidateUpdate_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var request = new TemplateUpdateRequest("welcome-email", "Welcome", TemplateType.Email, RetentionPolicy.Standard);

      var result = _validator.ValidateUpdate(request, templateExists: false, keyExists: false);

      result.HasError.Should().BeTrue();
      result.Messages.Should().Contain(m => m is NotFoundError);
   }

   [Theory]
   [InlineData(1, 25, true)]
   [InlineData(0, 25, false)]
   [InlineData(1, 0, false)]
   public void ValidateSearch_ShouldValidatePaging(int pageNumber, int pageSize, bool expectedSuccess)
   {
      var request = new TemplateSearchRequest(null, null, null, pageNumber, pageSize);

      var result = _validator.ValidateSearch(request);

      result.HasError.Should().Be(!expectedSuccess);
   }

   [Theory]
   [InlineData("en", "Valid subject", "Body", true)]
   [InlineData("", "Subject", "Body", false)]
   [InlineData("en", "", "Body", false)]
   [InlineData("en", "Subject", "", false)]
   public void ValidateEmailTranslation_ShouldValidateRequiredFields(string language, string subject, string body, bool expectedSuccess)
   {
      var request = new TemplateEmailTranslationRequest(language, subject, body);

      var result = _validator.ValidateEmailTranslation(request, templateExists: true, isEmailTemplate: true);

      result.HasError.Should().Be(!expectedSuccess);
   }

   [Fact]
   public void ValidateEmailTranslation_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var request = new TemplateEmailTranslationRequest("en", "Valid subject", "Body");

      var result = _validator.ValidateEmailTranslation(request, templateExists: false, isEmailTemplate: true);

      result.HasError.Should().BeTrue();
      result.Messages.Should().Contain(m => m is NotFoundError);
   }

   [Fact]
   public void ValidateEmailTranslation_ShouldReturnTypeMismatch_WhenTemplateIsNotEmail()
   {
      var request = new TemplateEmailTranslationRequest("en", "Valid subject", "Body");

      var result = _validator.ValidateEmailTranslation(request, templateExists: true, isEmailTemplate: false);

      result.HasError.Should().BeTrue();
      result.Messages.Should().Contain(m => m is TemplateTypeMismatchError);
   }

   private static TemplateCreateRequest CreateRequest()
   {
      return new TemplateCreateRequest("welcome-email", "Welcome", TemplateType.Email, RetentionPolicy.Standard);
   }
}
