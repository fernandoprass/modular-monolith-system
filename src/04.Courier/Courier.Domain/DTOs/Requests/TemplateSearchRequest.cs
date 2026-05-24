using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record TemplateSearchRequest(
   string? Key,
   string? Name,
   TemplateType? Type,
   int PageNumber = 1,
   int PageSize = 25);
