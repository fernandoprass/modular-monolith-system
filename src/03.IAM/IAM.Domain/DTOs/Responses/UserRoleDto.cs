namespace IAM.Domain.DTOs.Responses;

public record UserRoleDto(
   Guid Id,
   Guid RoleId,
   string Name,
   bool IsActive,
   bool IsDefault,
   DateTime StartsAt,
   DateTime? ExpiresAt,
   string AssignedBy,
   DateTime AssignedAt
 );
