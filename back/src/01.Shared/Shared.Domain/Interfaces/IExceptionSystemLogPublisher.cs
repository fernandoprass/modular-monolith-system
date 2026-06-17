using Microsoft.AspNetCore.Http;

namespace Shared.Domain.Interfaces;

public interface IExceptionSystemLogPublisher
{
   Task PublishAsync(
      string module,
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken = default);
}
