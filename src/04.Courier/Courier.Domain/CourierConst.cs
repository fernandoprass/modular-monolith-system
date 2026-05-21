namespace Courier.Domain;

public static partial class CourierConst
{
   public static class Database
   {
      public const string ConnectionString = "CourierDb";
      public const string DefaultName = "courier";
   }

   public static class Collection
   {
      public const string Emails = "emails";
      public const string EmailTemplates = "email_templates";
   }

   public static class EmailRetentionPoliciesTimeSpans
   {
      public const int Transient = 7;     // 7 Days
      public const int Operational = 30;  // 1 month
      public const int Standard= 90;      // 3 months
      public const int Extended = 365;    // 1 year
      public const int Compliance = 1825; // 5 years
   }

   public static class Entity
   {
      public const string Email = nameof(Entities.Email);
      public const string EmailTemplate = nameof(Entities.EmailTemplate);
   }

   public static class System
   {
      public const string ModuleName = "Courier";
   }
}
