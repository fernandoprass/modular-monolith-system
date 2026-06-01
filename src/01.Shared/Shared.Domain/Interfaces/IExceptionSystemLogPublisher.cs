using Shared.Domain.Enums;

namespace Shared.Domain.Interfaces;

public interface IExceptionSystemLogPublisher
{
   Task PublishAsync(
      string source,
      Exception exception,
      int statusCode,
      string? requestId,
      string? path,
      RetentionPolicy retentionPolicy,
      CancellationToken cancellationToken = default);
}
