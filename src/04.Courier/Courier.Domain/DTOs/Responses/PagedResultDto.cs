namespace Courier.Domain.DTOs.Responses;

public record PagedResultDto<T>(
   IEnumerable<T> Items,
   int PageNumber,
   int PageSize,
   int TotalCount,
   int TotalPages);
