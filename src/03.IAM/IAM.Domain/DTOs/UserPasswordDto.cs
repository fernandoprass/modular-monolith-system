using IAM.Domain.Entities;

namespace IAM.Domain.DTOs;

public sealed record UserPasswordDto
{
   public Guid Id { get; init; }
   public string Name { get; init; } = string.Empty;
   public string Email { get; init; } = string.Empty;
   public Guid OrganizationId { get; init; }
   public string OrganizationName { get; init; } = string.Empty;
   public bool OrganizationIsActive { get; init; }
   public string PasswordHash { get; init; } = string.Empty;
   public bool IsActive { get; init; }
   public bool IsSystemAdmin { get; init; } = false;
   public bool IsOrganizationAdmin { get; init; } = false;
   public int FailedLoginAttempts { get; init; }
   public DateTime CreatedAt { get; init; }
   public DateTime? EmailVerifiedAt { get; set; }
   public DateTime? LastLoginAt { get; set; }
   public DateTime? LockedOutUntil { get; set; }
   public IEnumerable<UserRole> UserRoles { get; set; } = [];
}
