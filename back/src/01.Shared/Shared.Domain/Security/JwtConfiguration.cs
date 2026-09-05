namespace Shared.Domain.Security;

public static class JwtConfiguration
{
   public static string GetRequiredSecret(string? jwtSecret)
   {
      if (string.IsNullOrWhiteSpace(jwtSecret))
      {
         throw new InvalidOperationException("JWT secret is required.");
      }

      return jwtSecret;
   }
}
