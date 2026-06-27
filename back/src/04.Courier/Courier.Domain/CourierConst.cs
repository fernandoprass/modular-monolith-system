namespace Courier.Domain;

public static partial class CourierConst
{
   public static class Database
   {
      public const string ConnectionString = "CourierDb";
      public const string DefaultName = "courier";
      public static class Collection
      {
         public const string Emails = "emails";
         public const string Notifications = "notifications";
         public const string Templates = "templates";
         public const string UserPreferences = "userPreferences";
      }
   }

   public static class RetentionPoliciesTimeSpans
   {
      public static class Email
      {
         public const int Operational = 30;  // 1 month
         public const int Standard = 90;     // 3 months
         public const int Extended = 365;    // 1 year
         public const int Compliance = 1825; // 5 years
         public const int LongTerm = 3965;   // 10 years
      }

      public static class Notification
      {
         public const int Operational = 7;  // 1 week
         public const int Standard = 30;    // 1 month
         public const int Extended = 90;    // 3 months
         public const int Compliance = 180; // 6 months
         public const int LongTerm = 360;   // 1 year
      }
   }

   public static class Entity
   {
      public const string Email = nameof(Entities.Email);
      public const string Notification = nameof(Entities.Notification);
      public const string Template = nameof(Entities.Template);
      public const string UserPreference = nameof(Entities.UserPreference);
   }

   public static class System
   {
      public const string ModuleName = "Courier";
   }

   public static class Event
   {
      public const int Version = 1;

      public static class Name
      {
         public const string MessageRequested = "courier.message.requested";
      }
   }

   public static class Redis
   {
      public const string MessageRequestsStream = "courier-message-requests";
      public const string EventFieldName = "event";
      public const string InitialStreamPosition = "0-0";
      public const string NewMessagesStreamPosition = ">";

      public const string MessageRequestConsumerGroup = "courier-message-request-consumer";
      public const string MessageRequestConsumerNamePrefix = "courier-message-request";

      public const int ReadBatchSize = 10;
      public const int EmptyStreamDelaySeconds = 1;
      public const int ErrorDelaySeconds = 5;
   }

   public static class Worker
   {
      public const int EmailDeliveryBatchSize = 10;
      public const int EmailDeliveryBatchDelaySeconds = 1;
      public const int EmailDeliveryErrorDelaySeconds = 5;
      public const int DefaultMaxRetries = 3;
   }

   public static class Logger
   {
      public static class Feature
      {
         public const string Emails = "emails";
      }

      public static class Action
      {
         public const string Queue = "queue";
         public const string Send = "send";
         public const string Fail = "fail";
      }
   }
}
