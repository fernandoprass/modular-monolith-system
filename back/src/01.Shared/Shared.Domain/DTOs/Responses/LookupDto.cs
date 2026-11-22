namespace Shared.Domain.DTOs.Responses;

public record LookupDto(Guid Id, string Type, string Code, bool IsActive,
   Guid? ParentId, Guid? OrganizationId, IReadOnlyCollection<LookupTranslationDto> Translations);

public record LookupTranslationDto(Guid Id, string LanguageCode, string Code,
   string Value, string? Description, bool IsActive, Guid LookupId);

public record LookupLiteDto(Guid Id, string Type, string Code, bool IsActive,
   Guid? ParentId, Guid? OrganizationId, string Value, string? Description, string? LanguageCode);
