using Courier.Domain.Entities;
using Courier.Domain.Enums;
using FluentAssertions;
using Shared.Domain.Enums;

namespace Courier.Domain.Tests.Entities;

public class TemplateTests
{
   [Fact]
   public void Create_ShouldNormalizeKeyAndName()
   {
      var createdBy = Guid.NewGuid();

      var template = Template.Create(
         " Welcome-Email ",
         " Welcome Email ",
         TemplateType.Email,
         RetentionPolicy.Standard,
         createdBy);

      template.Key.Should().Be("welcome-email");
      template.Name.Should().Be("Welcome Email");
      template.Type.Should().Be(TemplateType.Email);
      template.RetentionPolicy.Should().Be(RetentionPolicy.Standard);
      template.CreatedBy.Should().Be(createdBy);
   }

   [Fact]
   public void AddEmailTranslation_ShouldAddTranslation()
   {
      var template = CreateTemplate();
      var updatedBy = Guid.NewGuid();

      var result = template.AddEmailTranslation(" EN ", " Subject ", "<p>Body</p>", updatedBy);

      result.Should().BeTrue();
      template.UpdatedBy.Should().Be(updatedBy);
      template.EmailTranslations.Should().ContainSingle(translation =>
         translation.Language == "en" &&
         translation.Subject == "Subject" &&
         translation.Body == "<p>Body</p>" &&
         translation.IsHtml);
   }

   [Fact]
   public void AddEmailTranslation_ShouldReturnFalse_WhenLanguageExists()
   {
      var template = CreateTemplate();
      template.AddEmailTranslation("en", "Subject", "Body", Guid.NewGuid());

      var result = template.AddEmailTranslation(" EN ", "Other", "Other", Guid.NewGuid());

      result.Should().BeFalse();
      template.EmailTranslations.Should().ContainSingle();
   }

   [Fact]
   public void UpdateEmailTranslation_ShouldUpdateTranslation()
   {
      var template = CreateTemplate();
      var updatedBy = Guid.NewGuid();
      template.AddEmailTranslation("en", "Subject", "Body", Guid.NewGuid());

      var result = template.UpdateEmailTranslation(" EN ", " Updated ", "<p>Updated</p>", updatedBy);

      result.Should().BeTrue();
      template.UpdatedBy.Should().Be(updatedBy);
      template.EmailTranslations.Should().ContainSingle(translation =>
         translation.Language == "en" &&
         translation.Subject == "Updated" &&
         translation.Body == "<p>Updated</p>" &&
         translation.IsHtml);
   }

   [Fact]
   public void UpdateEmailTranslation_ShouldReturnFalse_WhenLanguageDoesNotExist()
   {
      var template = CreateTemplate();

      var result = template.UpdateEmailTranslation("en", "Subject", "Body", Guid.NewGuid());

      result.Should().BeFalse();
      template.EmailTranslations.Should().BeEmpty();
   }

   [Fact]
   public void RemoveTranslation_ShouldRemoveTranslation()
   {
      var template = CreateTemplate();
      var updatedBy = Guid.NewGuid();
      template.AddEmailTranslation("en", "Subject", "Body", Guid.NewGuid());

      var result = template.RemoveTranslation(" EN ", updatedBy);

      result.Should().BeTrue();
      template.UpdatedBy.Should().Be(updatedBy);
      template.EmailTranslations.Should().BeEmpty();
   }

   [Fact]
   public void RemoveTranslation_ShouldReturnFalse_WhenLanguageDoesNotExist()
   {
      var template = CreateTemplate();

      var result = template.RemoveTranslation("en", Guid.NewGuid());

      result.Should().BeFalse();
   }

   [Fact]
   public void RemoveTranslation_ShouldReturnFalse_WhenTemplateIsNotEmail()
   {
      var template = Template.Create(
         "comment-template",
         "Comment Template",
         TemplateType.Comment,
         RetentionPolicy.Standard,
         Guid.NewGuid());

      var result = template.RemoveTranslation("en", Guid.NewGuid());

      result.Should().BeFalse();
   }

   private static Template CreateTemplate()
   {
      return Template.Create(
         "welcome-email",
         "Welcome Email",
         TemplateType.Email,
         RetentionPolicy.Standard,
         Guid.NewGuid());
   }
}
