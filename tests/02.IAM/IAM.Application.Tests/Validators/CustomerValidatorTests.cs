using FluentAssertions;
using IAM.Application.Validators;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Enums;
using IAM.Domain.Messages.Errors;

namespace IAM.Application.Tests.Validators;

public class CustomerValidatorTests
{
   private readonly CustomerValidator _validator;

   public CustomerValidatorTests()
   {
      _validator = new CustomerValidator();
   }

   #region ValidateCreate Tests

   [Theory]
   [InlineData(CustomerType.Company, "Valid Company Name", "ABC123", false, true)] // Valid Company
   [InlineData(CustomerType.Individual, "Valid Individual", "1", false, true)]    // Valid Individual (Code rules are ignored)
   [InlineData(CustomerType.Company, "Valid Name", "DUP123", true, false)]    // Invalid: Duplicate Code  
   [InlineData(CustomerType.Company, "Valid Name", "AB", false, false)]       // Invalid: Company Code too short 
   [InlineData(CustomerType.Company, "Valid Name", "A_1", false, false)]      // Invalid: Company Code not alphanumeric    
   [InlineData(CustomerType.Company, "", "ABC123", false, false)]             // Invalid: Title empty (Assuming ValidatorTemplate.NameRules requires it)
   public void ValidateCreate_ShouldProcessAllRules(
       CustomerType type,
       string name,
       string code,
       bool codeExists,
       bool expectedSuccess)
   {
      //just to provid a user, validation is done in UserValidator
      var user = new CustomerUserCreateRequest(string.Empty, string.Empty, string.Empty);

      var request = new CustomerCreateRequest(type, name, code, "description", user);

      var result = _validator.ValidateCreate(request, codeExists);

      result.IsSuccess.Should().Be(expectedSuccess);

      if (!expectedSuccess && codeExists)
      {
         result.Messages.Should().Contain(m => m is DuplicateCustomerCodeError);
      }
   }

   #endregion

   #region ValidateUpdate Tests

   [Theory]
   [InlineData("Valid Name", true, true)]
   [InlineData("Valid Name", false, false)]
   [InlineData("", true, false)]
   [InlineData("Ab", true, false)] // Assuming min length 3 in NameRules
   public void ValidateUpdate_ShouldValidateName(string name, bool customerExists, bool expectedSuccess)
   {
      var request = new CustomerUpdateRequest(name, string.Empty, IsActive: true);

      var result = _validator.ValidateUpdate(request, customerExists);

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
      var request = new CustomerUpdateCodeRequest(code);

      var result = _validator.ValidateUpdateCode(request, newCodeExists);

      result.IsSuccess.Should().Be(expectedSuccess);

      if (!expectedSuccess && newCodeExists)
      {
         result.Messages.Should().Contain(m => m is DuplicateCustomerCodeError);
      }
   }

   #endregion
}
