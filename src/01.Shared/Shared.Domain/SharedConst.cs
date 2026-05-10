namespace Shared.Domain;

public static partial class SharedConst
{
   public static class Database
   {
      public const string ConnectionString = "SharedDb";
      public const string Schema = "shared";

      public const string TextType = "text";
      public const string UuidType = "uuid";
   }

   public static class Entity
   {
      public const string Parameter = nameof(Entities.Parameter);
      public const string ParameterOverride = nameof(Entities.ParameterOverride);
   }

   public static class System
   {
      public const string ModuleName = "Shared";
   }
}
