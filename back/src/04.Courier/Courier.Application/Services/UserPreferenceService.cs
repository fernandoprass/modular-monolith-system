using Courier.Application.Contracts;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.ValueObjects;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain.Messages;

namespace Courier.Application.Services;

public class UserPreferenceService(
   IUserPreferenceRepository userPreferenceRepository,
   IUserContext userContext) : IUserPreferenceService
{
   private readonly IUserPreferenceRepository _userPreferenceRepository = userPreferenceRepository;
   private readonly IUserContext _userContext = userContext;

   public async Task<Result<IReadOnlyCollection<UserPreferenceTemplateOptionDto>>> GetAsync(
      CancellationToken cancellationToken = default)
   {
      var preference = await _userPreferenceRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken);
      var templates = await _userPreferenceRepository.GetOptOutTemplateOptionsAsync(
         _userContext.Language,
         cancellationToken);

      if (preference == null)
      {
         return Result<IReadOnlyCollection<UserPreferenceTemplateOptionDto>>.Success(templates);
      }

      var result = templates
         .Select(template => template with
         {
            IsEmailEnabled = preference.IsGlobalEmailEnabled
               && preference.DisabledEmailTemplates.All(disabled =>
                  !IsSameTemplate(disabled, template.Module, template.Key)),
            IsNotificationEnabled = preference.IsGlobalNotificationEnabled
               && preference.DisabledNotificationTemplates.All(disabled =>
                  !IsSameTemplate(disabled, template.Module, template.Key))
         })
         .ToArray();

      return Result<IReadOnlyCollection<UserPreferenceTemplateOptionDto>>.Success(result);
   }

   public async Task<Result> UpdateAsync(
      UserPreferenceUpdateRequest request,
      CancellationToken cancellationToken = default)
   {
      var preference = await _userPreferenceRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken)
         ?? UserPreference.CreateDefault(_userContext.UserId);
      var requestedTemplates = request.Templates ?? [];

      preference.UpdateGlobalChannels(
         requestedTemplates.Any(template => template.IsEmailEnabled),
         requestedTemplates.Any(template => template.IsNotificationEnabled));

      preference.ReplaceTemplatePreferences(
         requestedTemplates
            .Where(template => !template.IsEmailEnabled)
            .Select(template => new UserPreferenceTemplate(template.Module, template.Key)),
         requestedTemplates
            .Where(template => !template.IsNotificationEnabled)
            .Select(template => new UserPreferenceTemplate(template.Module, template.Key)));

      await _userPreferenceRepository.UpdateAsync(preference, cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   private static bool IsSameTemplate(UserPreferenceTemplate template, string module, string key)
   {
      return template.Module.Equals(module, StringComparison.OrdinalIgnoreCase)
         && template.TemplateKey.Equals(key, StringComparison.OrdinalIgnoreCase);
   }
}
