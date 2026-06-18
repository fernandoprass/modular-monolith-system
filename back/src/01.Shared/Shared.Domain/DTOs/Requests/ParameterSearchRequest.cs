namespace Shared.Domain.DTOs.Requests;

public record ParameterSearchRequest(
   string? Module,
   string? Group,
   string? Name,
   string? Key,
   string? Title,
   int PageNumber = 1,
   int PageSize = 25
);