namespace Courier.Domain.DTOs.Requests;

public record UserPreferenceUpdateRequest(
   IReadOnlyCollection<UserPreferenceTemplateRequest> Templates);

public record UserPreferenceTemplateRequest(
   string Module,
   string Key,
   bool IsEmailEnabled,
   bool IsNotificationEnabled);
