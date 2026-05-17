namespace IAM.Domain;

public static partial class IamConst
{
   public static class Organization
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
      public const string Organization = nameof(Entities.Organization);
      public const string User = nameof(Entities.User);
      public const string Role = nameof(Entities.Role);
      public const string Permission = nameof(Entities.Permission);
   }

   public static class System
   {
      public const string ModuleName = "IAM";
   }
}
