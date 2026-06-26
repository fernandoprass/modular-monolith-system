namespace Courier.Application.Contracts;

public interface IEmailOutboxService
{
   Task<bool> ProcessNextPendingAsync(CancellationToken cancellationToken = default);
}
