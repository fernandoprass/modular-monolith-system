using FluentAssertions;
using IAM.Application.Validators;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Enums;
using IAM.Domain.Messages.Errors;

namespace IAM.Application.Tests.Validators;

public class OrganizationValidatorTests
{
   private readonly OrganizationValidator _validator;

   public OrganizationValidatorTests()
   {
      _validator = new OrganizationValidator();
   }

   #region ValidateCreate Tests

   [Theory]
   [InlineData(OrganizationType.Company, "Valid Company Name", "ABC123", false, true)] // Valid Company
   [InlineData(OrganizationType.Individual, "Valid Individual", "1", false, true)]    // Valid Individual (Code rules are ignored)
   [InlineData(OrganizationType.Company, "Valid Name", "DUP123", true, false)]    // Invalid: Duplicate Code  
   [InlineData(OrganizationType.Company, "Valid Name", "AB", false, false)]       // Invalid: Company Code too short 
   [InlineData(OrganizationType.Company, "Valid Name", "A_1", false, false)]      // Invalid: Company Code not alphanumeric    
   [InlineData(OrganizationType.Company, "", "ABC123", false, false)]             // Invalid: Title empty (Assuming ValidatorTemplate.NameRules requires it)
   public void ValidateCreate_ShouldProcessAllRules(
       OrganizationType type,
       string name,
       string code,
       bool codeExists,
       bool expectedSuccess)
   {
      //just to provid a user, validation is done in UserValidator
      var user = new OrganizationUserCreateRequest(string.Empty, string.Empty, string.Empty);

      var request = new OrganizationCreateRequest(type, name, code, "description", user);

      var result = _validator.ValidateCreate(request, codeExists);

      result.IsSuccess.Should().Be(expectedSuccess);

      if (!expectedSuccess && codeExists)
      {
         result.Messages.Should().Contain(m => m is OrganizationDuplicateCodeError);
      }
   }

   #endregion

   #region ValidateUpdate Tests

   [Theory]
   [InlineData("Valid Name", true, true)]
   [InlineData("Valid Name", false, false)]
   [InlineData("", true, false)]
   [InlineData("Ab", true, false)] // Assuming min length 3 in NameRules
   public void ValidateUpdate_ShouldValidateName(string name, bool organizationExists, bool expectedSuccess)
   {
      var request = new OrganizationUpdateRequest(name, string.Empty, IsActive: true);

      var result = _validator.ValidateUpdate(request, organizationExists);

      result.IsSuccess.Should().Be(expectedSuccess);
   }

   #endregion

   #region ValidateUpdateCode Tests

   [Theory]
   [InlineData("NEW123", false, true)] // Valid & New    
   [InlineData("OLD123", true, false)] // Duplicate   
   [InlineData("X1", false, false)]    // Invalid: Format (too short)     
   [InlineData("A@1", false, false)]   // Invalid: Format (special chars)
   public void ValidateUpdateCode_ShouldValidateCodeAndUniqueness(
       string code,
       bool newCodeExists,
       bool expectedSuccess)
   {
      var request = new OrganizationUpdateCodeRequest(code);

      var result = _validator.ValidateUpdateCode(request, newCodeExists);

      result.IsSuccess.Should().Be(expectedSuccess);

      if (!expectedSuccess && newCodeExists)
      {
         result.Messages.Should().Contain(m => m is OrganizationDuplicateCodeError);
      }
   }

   #endregion
}
