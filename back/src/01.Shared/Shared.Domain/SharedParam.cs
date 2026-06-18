namespace IAM.Domain;

public static class SharedParam
{
   private const string Module = "Shared";

   public static class System
   {
      private const string Group = "System";

      public const string DateFormat = $"{Module}.{Group}.{nameof(DateFormat)}";
   }
}
