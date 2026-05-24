namespace Courier.Domain;

public static class CourierParam
{
   private const string Module = "Courier";

   public static class EmailDelivery
   {
      private const string Group = "EmailDelivery";

      public const string MaxRetries = $"{Module}.{Group}.{nameof(MaxRetries)}";
   }
}
