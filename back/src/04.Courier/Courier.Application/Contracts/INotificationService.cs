using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Myce.Response;
using Shared.Domain.DTOs.Responses;

namespace Courier.Application.Contracts;

public interface INotificationService
{
   Task<Result<PagedResultDto<NotificationLiteDto>>> GetAsync(NotificationSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<NotificationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result<int>> GetUnreadCountAsync(CancellationToken cancellationToken = default);
   Task<Result> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
