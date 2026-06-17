using Shared.Domain;

namespace IAM.Domain.DTOs.Responses;

public sealed record UserDto
{
   public Guid Id { get; init; }
   public string Name { get; init; } = string.Empty;
   public string Email { get; init; } = string.Empty;
   public bool IsActive { get; init; }
   public bool IsSystemAdmin { get; init; }
   public bool IsOrganizationAdmin { get; init; }
   public DateTime CreatedAt { get; init; }
   public DateTime? EmailVerifiedAt { get; set; }
   public DateTime? LastLoginAt { get; set; }
   public string Language { get; init; } = LanguageOptions.English;
   public Guid OrganizationId { get; init; }
   public string OrganizationName { get; set; } = string.Empty;
}