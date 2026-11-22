namespace Shared.Domain.DTOs.Requests;

public record LookupSearchRequest(
   string? Type = null,
   Guid? ParentId = null,
   string? LanguageCode = null,
   bool? IsActive = true,
   bool RootsOnly = false,
   int PageNumber = SharedConst.Pagination.DefaultPageNumber,
   int PageSize = SharedConst.Pagination.DefaultPageSize);
