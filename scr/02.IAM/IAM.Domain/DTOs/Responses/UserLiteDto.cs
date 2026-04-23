namespace IAM.Domain.DTOs.Responses;

public sealed record UserLiteDto
{
   public Guid Id { get; init; }
   public string Name { get; init; } = string.Empty;
   public string Email { get; init; } = string.Empty;
   public bool IsActive { get; init; }
}