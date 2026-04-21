using FluentAssertions;

namespace Shared.Domain.Tests;

public class ParameterKeyTests
{
   [Theory]
   [InlineData("IAM.Security.PasswordExpire", "IAM", "Security", "PasswordExpire")]
   [InlineData("Finance.Tax.Rate", "Finance", "Tax", "Rate")]
   [InlineData("System.Logging.Level", "System", "Logging", "Level")]
   public void Constructor_WithSingleString_ShouldParseCorrectly(string key, string expectedModule, string expectedGroup, string expectedName)
   {
      var result = new ParameterKey(key);

      result.Module.Should().Be(expectedModule);
      result.Group.Should().Be(expectedGroup);
      result.Name.Should().Be(expectedName);
      result.Key.Should().Be(key);
   }

   [Theory]
   [InlineData("IAM", "Security", "PasswordExpire", "IAM.Security.PasswordExpire")]
   [InlineData("Finance", "Tax", "Rate", "Finance.Tax.Rate")]
   public void Constructor_WithThreeParts_ShouldAssembleCorrectly(string module, string group, string name, string expectedKey)
   {
      var result = new ParameterKey(module, group, name);

      result.Module.Should().Be(module);
      result.Group.Should().Be(group);
      result.Name.Should().Be(name);
      result.Key.Should().Be(expectedKey);
   }

   [Theory]
   [InlineData("Module.Group")]            // Too few parts
   [InlineData("Module.Group.Key.Extra")]  // Too many parts
   [InlineData("OnlyKey")]                 // No dots
   [InlineData("")]                        // Empty string
   public void Constructor_WithInvalidFormat_ShouldThrowArgumentException(string invalidKey)
   {
      Action act = () => _ = new ParameterKey(invalidKey);

      act.Should().Throw<ArgumentException>()
         .WithMessage("The parameter Key must follow the format 'Module.Group.Key'");
   }

   [Fact]
   public void ImplicitOperator_ShouldConvertStringToParameterKey()
   {
      string key = "Core.Settings.Enabled";

      ParameterKey result = key;

      result.Should().NotBeNull();
      result.Module.Should().Be("Core");
      result.Group.Should().Be("Settings");
      result.Name.Should().Be("Enabled");
      result.Key.Should().Be(key);
   }

   [Fact]
   public void ImplicitOperator_WithInvalidString_ShouldStillThrowException()
   {
      string invalidKey = "InvalidKey";

      Action act = () => { ParameterKey _ = invalidKey; };

      act.Should().Throw<ArgumentException>();
   }
}