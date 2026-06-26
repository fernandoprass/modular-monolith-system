using Courier.Domain.DTOs.Requests;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface ICourierMessageService
{
   Task<Result> QueueAsync(CourierMessageRequest request, CancellationToken cancellationToken = default);
}
