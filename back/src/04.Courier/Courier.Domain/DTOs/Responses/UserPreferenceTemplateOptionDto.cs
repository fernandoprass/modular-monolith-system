namespace Courier.Domain.DTOs.Responses;

public record UserPreferenceTemplateOptionDto(
   string Module,
   string Key,
   string Name,
   bool IsEmailEnabled,
   bool IsNotificationEnabled);
