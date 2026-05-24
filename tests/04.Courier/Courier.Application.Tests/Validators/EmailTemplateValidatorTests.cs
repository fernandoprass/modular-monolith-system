using Courier.Application.Validators;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Enums;
using Courier.Domain.Messages;
using FluentAssertions;
using Shared.Domain.Messages;

namespace Courier.Application.Tests.Validators;

public class EmailTemplateValidatorTests
{
   private readonly EmailTemplateValidator _validator = new();

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
      result.Messages.Should().ContainSingle(m => m is EmailTemplateDuplicateKeyError);
   }

   [Fact]
   public void ValidateUpdate_ShouldReturnNotFound_WhenTemplateDoesNotExist()
   {
      var request = new EmailTemplateUpdateRequest("welcome-email", "Welcome", EmailRetentionPolicy.Standard);

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
      var request = new EmailTemplateSearchRequest(null, pageNumber, pageSize);

      var result = _validator.ValidateSearch(request);

      result.HasError.Should().Be(!expectedSuccess);
   }

   [Theory]
   [InlineData("en", "Subject", "Body", true)]
   [InlineData("", "Subject", "Body", false)]
   [InlineData("en", "", "Body", false)]
   [InlineData("en", "Subject", "", false)]
   public void ValidateTranslation_ShouldValidateRequiredFields(string language, string subject, string body, bool expectedSuccess)
   {
      var request = new EmailTemplateTranslationRequest(language, subject, body);

      var result = _validator.ValidateTranslation(request);

      result.HasError.Should().Be(!expectedSuccess);
   }

   private static EmailTemplateCreateRequest CreateRequest()
   {
      return new EmailTemplateCreateRequest("welcome-email", "Welcome", EmailRetentionPolicy.Standard);
   }
}
