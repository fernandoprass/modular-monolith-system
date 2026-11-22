namespace Shared.Domain.DTOs.Requests;

// Type, organization ownership, and the original lookup code cannot be changed.
public record LookupUpdateRequest(
   Guid? ParentId,
   bool IsActive,
   IReadOnlyList<LookupTranslationRequest> Translations);
