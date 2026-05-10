namespace IAM.Domain.DTOs.Responses;

public sealed record LoginResponse
{
   public string Token { get; init; } = string.Empty;
   public DateTime ExpiresAt { get; init; }
   public UserDto User { get; init; } = null!;

   public LoginResponse(string token, DateTime expiresAt, UserDto userDto)
   {
      Token = token;
      ExpiresAt = expiresAt;
      User = userDto;
   }
}