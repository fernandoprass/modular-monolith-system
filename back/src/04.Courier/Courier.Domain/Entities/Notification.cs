using Courier.Domain.Enums;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Courier.Domain.Entities
{
   public class Notification : Entity
   {
      public Guid OrganizationId { get; private set; }
      public Guid UserId { get; private set; }
      public string Module { get; private set; } = string.Empty;
      public string Feature { get; private set; } = string.Empty;
      public string TemplateKey { get; private set; } = string.Empty;
      public string Title { get; private set; } = string.Empty;
      public string Message { get; private set; } = string.Empty;
      public string ActionLink { get; private set; } = string.Empty;
      public DateTime CreatedAt { get; private set; }
      public DateTime? ReadAt { get; private set; }
      public DateTime ExpiresAt { get; private set; }
      public NotificationStatus Status { get; private set; } = NotificationStatus.Unread;

      // Private constructor for MongoDB deserialization
      private Notification() { }

      public static Notification Create(
          Guid organizationId,
          Guid userId,
          string module,
          string feature,
          string templateKey,
          string title,
          string message,
          string actionLink,
          RetentionPolicy retentionPolicy)
      {
         return new Notification
         {
            Id = Guid.CreateVersion7(),
            OrganizationId = organizationId,
            UserId = userId,
            Module = module.Trim(),
            Feature = feature.Trim(),
            TemplateKey = templateKey.Trim(),
            Title = title,
            Message = message,
            ActionLink = actionLink.Trim(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRetentionDays(retentionPolicy)),
            Status = NotificationStatus.Unread
         };
      }

      public void MarkAsRead()
      {
         if (Status == NotificationStatus.Unread)
         {
            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
         }
      }

      private static int GetRetentionDays(RetentionPolicy retentionPolicy)
      {
         return retentionPolicy switch
         {
            RetentionPolicy.Operational => CourierConst.RetentionPoliciesTimeSpans.Notification.Operational,
            RetentionPolicy.Standard => CourierConst.RetentionPoliciesTimeSpans.Notification.Standard,
            RetentionPolicy.Extended => CourierConst.RetentionPoliciesTimeSpans.Notification.Extended,
            RetentionPolicy.Compliance => CourierConst.RetentionPoliciesTimeSpans.Notification.Compliance,
            RetentionPolicy.LongTerm => CourierConst.RetentionPoliciesTimeSpans.Notification.LongTerm,
            _ => CourierConst.RetentionPoliciesTimeSpans.Notification.Standard
         };
      }
   }
}
