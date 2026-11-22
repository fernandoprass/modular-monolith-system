namespace Shared.Domain.DTOs.Requests;

public record LookupTranslationRequest(
   string LanguageCode, string Code, string Value, string? Description, bool IsActive);
