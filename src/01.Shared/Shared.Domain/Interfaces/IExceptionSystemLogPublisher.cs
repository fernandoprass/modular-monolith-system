namespace Shared.Domain.Interfaces;

public interface IExceptionSystemLogPublisher
{
   Task PublishAsync(
      string source,
      Exception exception,
      int statusCode,
      string? requestId,
      string? path,
      CancellationToken cancellationToken = default);
}
