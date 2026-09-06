using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Mappers;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Messages;

namespace Courier.Application.Services;

public class NotificationService(
   INotificationRepository notificationRepository,
   INotificationValidator notificationValidator,
   IUserContext userContext) : BaseService(userContext), INotificationService
{
   private readonly INotificationRepository _notificationRepository = notificationRepository;
   private readonly INotificationValidator _notificationValidator = notificationValidator;

   public async Task<Result<PagedResultDto<NotificationLiteDto>>> GetAsync(
      NotificationSearchRequest request,
      CancellationToken cancellationToken = default)
   {
      var validation = _notificationValidator.ValidateSearch(request);

      if (validation.HasError)
      {
         return Result<PagedResultDto<NotificationLiteDto>>.Failure(validation.Messages);
      }

      var searchRequest = _userContext.IsSystemAdmin
         ? request
         : request with { OrganizationId = _userContext.OrganizationId, UserId = _userContext.UserId };

      var notifications = await _notificationRepository.GetAsync(searchRequest, cancellationToken);
      return Result<PagedResultDto<NotificationLiteDto>>.Success(notifications);
   }

   public async Task<Result<NotificationDto>> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);

      if (notification == null)
      {
         return Result<NotificationDto>.Failure(new NotFoundError(CourierConst.Entity.Notification));
      }

      if (!CanAccessNotification(notification))
      {
         return Result<NotificationDto>.Failure(new UnauthorizedAccessError());
      }

      return Result<NotificationDto>.Success(notification.ToNotificationDto());
   }

   public async Task<Result<int>> GetUnreadCountAsync(CancellationToken cancellationToken = default)
   {
      var count = await _notificationRepository.GetUnreadCountAsync(
         _userContext.OrganizationId,
         _userContext.UserId,
         cancellationToken);

      return Result<int>.Success(count);
   }

   public async Task<Result> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);

      if (notification == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.Notification));
      }

      if (!CanAccessNotification(notification))
      {
         return Result.Failure(new UnauthorizedAccessError());
      }

      notification.MarkAsRead();
      await _notificationRepository.UpdateAsync(notification, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);

      if (notification == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.Notification));
      }

      if (!CanAccessNotification(notification))
      {
         return Result.Failure(new UnauthorizedAccessError());
      }

      await _notificationRepository.DeleteAsync(id, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   private bool CanAccessNotification(Notification notification)
   {
      return _userContext.IsSystemAdmin ||
         (notification.OrganizationId == _userContext.OrganizationId &&
          notification.UserId == _userContext.UserId);
   }
}
