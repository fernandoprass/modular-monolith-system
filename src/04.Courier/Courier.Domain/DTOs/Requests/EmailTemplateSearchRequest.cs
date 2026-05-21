namespace Courier.Domain.DTOs.Requests;

public record EmailTemplateSearchRequest(
   string? Key,
   int PageNumber = 1,
   int PageSize = 25);
