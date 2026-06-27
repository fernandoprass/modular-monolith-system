using Courier.Application.Services;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using Shared.Application.Contracts;

namespace Courier.Application.Tests.Services;

public class UserPreferenceServiceTests
{
   private readonly IUserPreferenceRepository _userPreferenceRepository = Substitute.For<IUserPreferenceRepository>();
   private readonly IUserContext _userContext = Substitute.For<IUserContext>();
   private readonly UserPreferenceService _service;
   private readonly Guid _userId = Guid.NewGuid();

   public UserPreferenceServiceTests()
   {
      _userContext.UserId.Returns(_userId);
      _userContext.Language.Returns("en");
      _service = new UserPreferenceService(_userPreferenceRepository, _userContext);
   }

   [Fact]
   public async Task GetAsync_ShouldEnableAllTemplates_WhenUserHasNoDocument()
   {
      var templates = new[]
      {
         new UserPreferenceTemplateOptionDto("iam", "welcome", "Welcome", true, true)
      };

      _userPreferenceRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns((UserPreference?)null);
      _userPreferenceRepository.GetOptOutTemplateOptionsAsync("en", Arg.Any<CancellationToken>()).Returns(templates);

      var result = await _service.GetAsync(TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().ContainSingle(template =>
         template.IsEmailEnabled && template.IsNotificationEnabled);
   }

   [Fact]
   public async Task GetAsync_ShouldApplySavedTemplatePreferences()
   {
      var preference = UserPreference.CreateDefault(_userId);
      var templates = new[]
      {
         new UserPreferenceTemplateOptionDto("iam", "welcome", "Welcome", true, true)
      };

      preference.DisableEmailTemplatePreference("iam", "welcome");
      _userPreferenceRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(preference);
      _userPreferenceRepository.GetOptOutTemplateOptionsAsync("en", Arg.Any<CancellationToken>()).Returns(templates);

      var result = await _service.GetAsync(TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().ContainSingle(template =>
         template.Module == "iam"
         && template.Key == "welcome"
         && !template.IsEmailEnabled
         && template.IsNotificationEnabled);
   }

   [Fact]
   public async Task UpdateAsync_ShouldCreatePreference_WhenUserHasNoDocument()
   {
      var request = new UserPreferenceUpdateRequest(
         [
            new UserPreferenceTemplateRequest(
               "iam",
               "welcome",
               IsEmailEnabled: true,
               IsNotificationEnabled: false),
            new UserPreferenceTemplateRequest(
               "iam",
               "security-alert",
               IsEmailEnabled: false,
               IsNotificationEnabled: true)
         ]);

      _userPreferenceRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns((UserPreference?)null);

      var result = await _service.UpdateAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _userPreferenceRepository.Received(1).UpdateAsync(
         Arg.Is<UserPreference>(preference =>
            preference.UserId == _userId
            && preference.IsGlobalEmailEnabled
            && preference.IsGlobalNotificationEnabled
            && preference.DisabledEmailTemplates.Count == 1
            && preference.DisabledEmailTemplates.Any(template => template.TemplateKey == "security-alert")
            && preference.DisabledNotificationTemplates.Count == 1
            && preference.DisabledNotificationTemplates.Any(template => template.TemplateKey == "welcome")),
         Arg.Any<CancellationToken>());
      await _userPreferenceRepository.DidNotReceive().GetOptOutTemplateOptionsAsync(
         Arg.Any<string>(),
         Arg.Any<CancellationToken>());
   }
}
