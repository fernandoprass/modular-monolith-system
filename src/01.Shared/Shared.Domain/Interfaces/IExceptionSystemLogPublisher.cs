using Microsoft.AspNetCore.Http;

namespace Shared.Domain.Interfaces;

public interface IExceptionSystemLogPublisher
{
   Task PublishAsync(
      string source,
      HttpRequest httpRequest,
      Exception exception,
      int statusCode,
      string? requestId,
      CancellationToken cancellationToken = default);
}
