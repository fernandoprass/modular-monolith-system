using FluentAssertions;
using IAM.Application.Validators;
using IAM.Domain.Messages;
using Myce.FluentValidator;

namespace IAM.Application.Tests.Validators;

public class ValidatorTemplateTests
{
   private class Dummy { public string Value { get; set; } }

   #region NameRules Tests

   [Theory]
   [InlineData("John Doe", true)]
   [InlineData("Ab", false)]   // Too short (Min 3)
   [InlineData("", false)]     // Required
   [InlineData(null, false)]   // Required
   public void NameRules_ShouldValidateCorrectly(string input, bool expectedSuccess)
   {
      var validator = new FluentValidator<Dummy>()
          .RuleFor(x => x.Value).ApplyTemplate(ValidatorTemplates.NameRules);

      var result = validator.Validate(new Dummy { Value = input });

      result.Should().Be(expectedSuccess);
   }

   #endregion

   #region EmailRules Tests

   [Theory]
   [InlineData("test@domain.com", true)]
   [InlineData("user.name@sub.domain.org", true)]
   [InlineData("invalid-email", false)]
   [InlineData("@domain.com", false)]
   [InlineData("test@", false)]
   [InlineData("", false)]
   public void EmailRules_ShouldValidateCorrectly(string input, bool expectedSuccess)
   {
      var validator = new FluentValidator<Dummy>()
          .RuleFor(x => x.Value).ApplyTemplate(ValidatorTemplates.EmailRules);

      var result = validator.Validate(new Dummy { Value = input });

      result.Should().Be(expectedSuccess);
   }

   #endregion

   #region PasswordRules Tests

   [Theory]
   [InlineData("Pass123!", true, "")]// Valid: Meets all 5 criteria   
   [InlineData("P1s!", false, IamTranslatedMessagesProvider.PasswordMinLengthError)]            // Invalid: Too short (< 8)  
   [InlineData("pass123!", false, IamTranslatedMessagesProvider.PasswordMissingUppercaseError)] // Invalid: Missing Uppercase  
   [InlineData("PASS123!", false, IamTranslatedMessagesProvider.PasswordMissingLowercaseError)] // Invalid: Missing Lowercase  
   [InlineData("Password!", false, IamTranslatedMessagesProvider.PasswordMissingDigitError)]    // Invalid: Missing Digit  
   [InlineData("Password123", false, IamTranslatedMessagesProvider.PasswordMissingSpecialError)]// Invalid: Missing Special Char   
   [InlineData("", false, "")]                                          // Invalid: Null or Empty
   public void PasswordRules_ShouldValidateComplexRequirements(
       string input,
       bool expectedSuccess,
       string expectedErrorCode)
   {
      var validator = new FluentValidator<Dummy>()
          .RuleFor(x => x.Value).ApplyTemplate(ValidatorTemplates.PasswordRules);

      var isValid = validator.Validate(new Dummy { Value = input });

      isValid.Should().Be(expectedSuccess);

      if (!expectedSuccess && expectedErrorCode != string.Empty)
      {
         validator.Messages.Should().Contain(m => m.Code.Equals(expectedErrorCode));
      }
   }

   [Theory]
   [InlineData("Pass123#")]
   [InlineData("Pass123?")]
   [InlineData("Pass123@")]
   [InlineData("Pass123-")]
   [InlineData("Pass123.")]
   public void PasswordRules_ShouldAcceptAllDefinedSpecialCharacters(string password)
   {
      var validator = new FluentValidator<Dummy>()
          .RuleFor(x => x.Value).ApplyTemplate(ValidatorTemplates.PasswordRules);

      var isValid = validator.Validate(new Dummy { Value = password });

      isValid.Should().BeTrue();
   }

   #endregion
}