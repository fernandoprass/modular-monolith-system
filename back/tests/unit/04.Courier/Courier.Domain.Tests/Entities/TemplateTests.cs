using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;
using FluentAssertions;
using Shared.Domain.Enums;

namespace Courier.Domain.Tests.Entities;

public class TemplateTests
{
   [Fact]
   public void Create_ShouldNormalizeMetadataAndSetAuditFields()
   {
      var createdBy = Guid.NewGuid();

      var template = Template.Create(
         " IAM ",
         " User-Welcome ",
         true,
         NotificationSeverity.Warning,
         RetentionPolicy.Standard,
         createdBy);

      template.Module.Should().Be("iam");
      template.Key.Should().Be("user-welcome");
      template.IsAllowingOptOut.Should().BeTrue();
      template.Severity.Should().Be(NotificationSeverity.Warning);
      template.RetentionPolicy.Should().Be(RetentionPolicy.Standard);
      template.CreatedBy.Should().Be(createdBy);
      template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
   }

   [Fact]
   public void AddTranslation_ShouldStoreEmailAndNotificationUnderOneLanguage()
   {
      var template = CreateTemplate();
      var updatedBy = Guid.NewGuid();
      var translation = CreateTranslation("pt-br");

      var result = template.AddTranslation(translation, updatedBy);

      result.Should().BeTrue();
      template.Translations.Should().ContainSingle(item =>
         item.Language == "pt-BR"
         && item.Name == "User welcome"
         && item.Email != null
         && item.Email.Subject == "Welcome {{user.name}}"
         && item.Email.IsHtml
         && item.Notification != null
         && item.Notification.Title == "Account created"
         && item.Notification.ActionLink == "/profile");
      template.UpdatedBy.Should().Be(updatedBy);
   }

   [Fact]
   public void AddTranslation_ShouldReturnFalse_WhenLanguageExists()
   {
      var template = CreateTemplate();
      template.AddTranslation(CreateTranslation("pt-BR"), Guid.NewGuid());

      var result = template.AddTranslation(CreateTranslation("PT-br"), Guid.NewGuid());

      result.Should().BeFalse();
      template.Translations.Should().ContainSingle();
   }

   [Fact]
   public void UpdateTranslation_ShouldReplaceBothChannels()
   {
      var template = CreateTemplate();
      var updatedBy = Guid.NewGuid();
      template.AddTranslation(CreateTranslation("en"), Guid.NewGuid());
      var updated = TemplateTranslation.Create(
         "en",
         "Updated name",
         TemplateTranslationEmail.Create("Updated subject", "Plain body"),
         null);

      var result = template.UpdateTranslation(" EN ", updated, updatedBy);

      result.Should().BeTrue();
      template.Translations.Should().ContainSingle(item =>
         item.Name == "Updated name"
         && item.Email != null
         && item.Email.Subject == "Updated subject"
         && !item.Email.IsHtml
         && item.Notification == null);
      template.UpdatedBy.Should().Be(updatedBy);
   }

   [Fact]
   public void UpdateTranslation_ShouldReturnFalse_WhenLanguageDoesNotExist()
   {
      var template = CreateTemplate();

      var result = template.UpdateTranslation("en", CreateTranslation("en"), Guid.NewGuid());

      result.Should().BeFalse();
      template.Translations.Should().BeEmpty();
   }

   [Fact]
   public void RemoveTranslation_ShouldRemoveLanguageNode()
   {
      var template = CreateTemplate();
      var updatedBy = Guid.NewGuid();
      template.AddTranslation(CreateTranslation("en"), Guid.NewGuid());

      var result = template.RemoveTranslation(" EN ", updatedBy);

      result.Should().BeTrue();
      template.Translations.Should().BeEmpty();
      template.UpdatedBy.Should().Be(updatedBy);
   }

   private static Template CreateTemplate()
   {
      return Template.Create(
         "iam",
         "user-welcome",
         false,
         NotificationSeverity.Information,
         RetentionPolicy.Standard,
         Guid.NewGuid());
   }

   private static TemplateTranslation CreateTranslation(string language)
   {
      return TemplateTranslation.Create(
         language,
         "User welcome",
         TemplateTranslationEmail.Create("Welcome {{user.name}}", "<p>Hello</p>"),
         TemplateTranslationNotification.Create("Account created", "Open your profile", "/profile"));
   }
}
