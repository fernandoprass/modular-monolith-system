using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Shared.Domain.DTOs.Responses;

namespace Courier.Domain.Interfaces.Repositories;

public interface INotificationRepository
{
   Task<PagedResultDto<NotificationLiteDto>> GetAsync(NotificationSearchRequest request, CancellationToken cancellationToken = default);
   Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<int> GetUnreadCountAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
   Task<Guid> AddAsync(Notification notification, CancellationToken cancellationToken = default);
   Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
}
