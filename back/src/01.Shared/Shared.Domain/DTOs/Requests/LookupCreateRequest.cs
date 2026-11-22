namespace Shared.Domain.DTOs.Requests;

public record LookupCreateRequest(
   string Type,
   Guid? ParentId,
   Guid? OrganizationId,
   bool IsActive,
   IReadOnlyList<LookupTranslationRequest> Translations);
