namespace IAM.Domain;

public static partial class IamConst
{
   public static class Customer
   {
      public const byte RandomCodeSize = 10;
   }

   public static class Database
   {
      public const string ConnectionString = "IamDb";
      public const string Schema = "iam";

      public const string TextType = "text";
      public const string UuidType = "uuid";
   }

   public static class Entity
   {
      public const string Customer = nameof(Entities.Customer);
      public const string User = nameof(Entities.User);
      public const string Role = nameof(Entities.Role);
      public const string Feature = nameof(Entities.Permission);
   }

   public class Security
   {
      public static class Claim
      {
         public const string UserOwnerId = "userOwnerId";
         public const string IsSystemAdmin = "isSystemAdmin";
         public const string Issuer = "IAM.API";
         public const string Audience = "IAM.Client";
      }
   }
   public static class System
   {
      public const string ModuleName = "IAM";
   }
}
