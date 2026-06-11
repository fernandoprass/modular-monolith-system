namespace Shared.Domain;

public static partial class SharedConst
{
   public static class Database
   {
      public const string ConnectionString = "SharedDb";
      public const string Schema = "shared";

      public static class PostgreSQL
      {
         public const string TextType = "text";
         public const string UuidType = "uuid";
         public const string CurrentTimeStamp = "CURRENT_TIMESTAMP";
      }
   }

   public static class Entity
   {
      public const string Parameter = nameof(Entities.Parameter);
      public const string ParameterOverride = nameof(Entities.ParameterOverride);
   }

   public static class Event
   {
      public const int Version = 1;

      public static class Name
      {
         public const string AuditLogRequested = "sentinel.audit-log.requested";
         public const string SystemLogRequested = "sentinel.system-log.requested";
         public const string EmailRequested = "courier.email.requested";
         public const string NotificationRequested = "courier.notification.requested";
      }
   }

   public static class Logger
   {
      public static class Feature
      {
         public const string Parameters = "parameters";
         public const string Security = "security";
      }

      public static class Action
      {
         public const string DeleteOverride = "delete-override";
         public const string SaveOverride = "save-override";
         public const string Update = "update";
         public const string UnauthorizedResourceAccess = "unauthorized-resource-access";
      }
   }

   public static class Pagination
   {
      public const int DefaultPageNumber = 1;
      public const int DefaultPageSize = 25;
      public const int MaxPageSize = 200;
   }

   public static class Redis
   {
      public const string ConnectionString = "Redis";
      public const string EventFieldName = "event";

      public const string AuditLogEventsStream = "audit-log-events";
      public const string SystemLogEventsStream = "system-log-events";
      public const string EmailRequestsStream = "courier-email-requests";
      public const string NotificationEventsChannel = "notification-events";
      public const string CacheKeyPrefixForParameter = "param:";
      public const string CacheKeyPrefixForRole = "role:";
   }

   public class Security
   {
      public static class Claim
      {
         public const string UserOwnerId = "userOwnerId";
         public const string IsSystemAdmin = "isSystemAdmin";
         public const string IsOrganizationAdmin = "isOrganizationAdmin";
         public const string Issuer = "IAM.API";
         public const string Audience = "IAM.Client";
         public const string Language = "language";
         public const string Role = "role";
      }
   }

   public static class System
   {
      public const string ModuleName = "Shared";
      public const string DefaultLanguage = "en";
   }
}
