using Courier.Domain.Entities;
using Courier.Domain.ValueObjects;
using FluentAssertions;

namespace Courier.Domain.Tests.Entities;

public class UserPreferenceTests
{
   [Fact]
   public void CreateDefault_ShouldEnableAllChannels()
   {
      var userId = Guid.NewGuid();

      var preference = UserPreference.CreateDefault(userId);

      preference.UserId.Should().Be(userId);
      preference.IsGlobalEmailEnabled.Should().BeTrue();
      preference.IsGlobalNotificationEnabled.Should().BeTrue();
      preference.DisabledEmailTemplates.Should().BeEmpty();
      preference.DisabledNotificationTemplates.Should().BeEmpty();
   }

   [Fact]
   public void DisableTemplatePreference_ShouldBlockOnlySelectedChannel()
   {
      var preference = UserPreference.CreateDefault(Guid.NewGuid());

      preference.DisableEmailTemplatePreference("IAM", "Welcome");
      preference.DisableNotificationTemplatePreference("iam", "locked");

      preference.IsEmailEnabledForTemplate("iam", "welcome").Should().BeFalse();
      preference.IsNotificationEnabledForTemplate("iam", "welcome").Should().BeTrue();
      preference.IsEmailEnabledForTemplate("iam", "locked").Should().BeTrue();
      preference.IsNotificationEnabledForTemplate("iam", "locked").Should().BeFalse();
   }

   [Fact]
   public void ReplaceTemplatePreferences_ShouldReplaceExistingLists()
   {
      var preference = UserPreference.CreateDefault(Guid.NewGuid());

      preference.DisableEmailTemplatePreference("iam", "old");
      preference.ReplaceTemplatePreferences(
         [new UserPreferenceTemplate("iam", "new-email")],
         [new UserPreferenceTemplate("courier", "new-notification")]);

      preference.DisabledEmailTemplates.Should().ContainSingle(template => template.TemplateKey == "new-email");
      preference.DisabledNotificationTemplates.Should().ContainSingle(template => template.TemplateKey == "new-notification");
      preference.IsEmailEnabledForTemplate("iam", "old").Should().BeTrue();
   }

   [Fact]
   public void UpdateGlobalChannels_ShouldNotClearTemplatePreferences()
   {
      var preference = UserPreference.CreateDefault(Guid.NewGuid());

      preference.DisableEmailTemplatePreference("iam", "welcome");
      preference.UpdateGlobalChannels(emailEnabled: false, notificationEnabled: true);
      preference.UpdateGlobalChannels(emailEnabled: true, notificationEnabled: true);

      preference.IsEmailEnabledForTemplate("iam", "welcome").Should().BeFalse();
   }
}
