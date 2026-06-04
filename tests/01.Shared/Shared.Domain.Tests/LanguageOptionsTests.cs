using FluentAssertions;
using Shared.Domain;

namespace Shared.Domain.Tests;

public class LanguageOptionsTests
{
   [Fact]
   public void AllowedLanguages_ShouldContainThreeSupportedLanguages()
   {
      LanguageOptions.AllowedLanguages.Should().HaveCount(3);
      LanguageOptions.AllowedLanguages.Should().Contain(LanguageOptions.English);
      LanguageOptions.AllowedLanguages.Should().Contain(LanguageOptions.PortugueseBrazil);
      LanguageOptions.AllowedLanguages.Should().Contain(LanguageOptions.Spanish);
   }

   [Theory]
   [InlineData(LanguageOptions.English, true)]
   [InlineData(LanguageOptions.PortugueseBrazil, true)]
   [InlineData(LanguageOptions.Spanish, true)]
   [InlineData("pt", false)]
   [InlineData("fr", false)]
   [InlineData("DE", false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData(null, false)]
   public void IsSupported_WithVariousLanguages_ShouldReturnCorrectly(string? language, bool expected)
   {
      var result = LanguageOptions.IsSupported(language);

      result.Should().Be(expected);
   }

   [Theory]
   [InlineData("EN", true)]          // Uppercase should normalize to "en"
   [InlineData("  en  ", true)]      // With spaces should normalize
   [InlineData("pt-br", true)]       // Lowercase variant should normalize to "pt-BR"
   [InlineData("PT-BR", true)]       // Uppercase variant should normalize
   [InlineData("  es  ", true)]      // Spanish with spaces
   public void IsSupported_WithCaseVariations_ShouldNormalize(string language, bool expected)
   {
      var result = LanguageOptions.IsSupported(language);

      result.Should().Be(expected);
   }

   [Theory]
   [InlineData(LanguageOptions.English, LanguageOptions.English)]
   [InlineData("EN", LanguageOptions.English)]
   [InlineData("  en  ", LanguageOptions.English)]
   [InlineData(LanguageOptions.PortugueseBrazil, LanguageOptions.PortugueseBrazil)]
   [InlineData("PT-BR", LanguageOptions.PortugueseBrazil)]
   [InlineData("pt-br", LanguageOptions.PortugueseBrazil)]
   [InlineData(LanguageOptions.Spanish, LanguageOptions.Spanish)]
   [InlineData("ES", LanguageOptions.Spanish)]
   public void Normalize_WithVariousFormats_ShouldNormalizeCorrectly(string language, string expected)
   {
      var result = LanguageOptions.Normalize(language);

      result.Should().Be(expected);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void Normalize_WithNullOrEmptyString_ShouldReturnEnglishDefault(string language)
   {
      var result = LanguageOptions.Normalize(language);

      result.Should().Be(LanguageOptions.English);
   }

   [Theory]
   [InlineData("fr")]
   [InlineData("de")]
   [InlineData("pt")]
   [InlineData("unsupported")]
   public void Normalize_WithUnsupportedLanguage_ShouldStillNormalizeForConsistency(string language)
   {
      var result = LanguageOptions.Normalize(language);

      result.Should().Be(language.ToLowerInvariant());
   }

   [Fact]
   public void English_Constant_ShouldBeCorrectValue()
   {
      LanguageOptions.English.Should().Be("en");
   }

   [Fact]
   public void PortugueseBrazil_Constant_ShouldBeCorrectValue()
   {
      LanguageOptions.PortugueseBrazil.Should().Be("pt-br");
   }

   [Fact]
   public void Spanish_Constant_ShouldBeCorrectValue()
   {
      LanguageOptions.Spanish.Should().Be("es");
   }
}
