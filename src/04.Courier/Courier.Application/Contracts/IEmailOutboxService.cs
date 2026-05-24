using Courier.Domain.DTOs.Requests;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface IEmailOutboxService
{
   Task<Result<Guid>> QueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default);
   Task<bool> ProcessNextPendingAsync(CancellationToken cancellationToken = default);
}
