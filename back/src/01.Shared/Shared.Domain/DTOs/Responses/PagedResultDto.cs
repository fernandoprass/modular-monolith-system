namespace Shared.Domain.DTOs.Responses;

public record PagedResultDto<T>(
   IReadOnlyCollection<T> Items,
   int PageNumber,
   int PageSize,
   long TotalCount,
   int TotalPages);
