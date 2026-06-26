using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;

namespace Courier.Domain.Mappers;

public static class NotificationMappers
{
   public static NotificationDto ToNotificationDto(this Notification notification)
   {
      return new NotificationDto(
         notification.Id,
         notification.OrganizationId,
         notification.UserId,
         notification.Module,
         notification.Feature,
         notification.TemplateKey,
         notification.Title,
         notification.Message,
         notification.ActionLink,
         notification.Status,
         notification.CreatedAt,
         notification.ReadAt,
         notification.ExpiresAt);
   }
}
