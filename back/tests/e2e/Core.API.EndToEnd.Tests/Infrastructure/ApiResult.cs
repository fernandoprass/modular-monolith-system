namespace Core.API.EndToEnd.Tests.Infrastructure;

internal sealed record ApiResult<T> where T : class
{
   public T? Data { get; init; }
   public bool HasError { get; init; }
   public bool IsSuccess { get; init; }
}
