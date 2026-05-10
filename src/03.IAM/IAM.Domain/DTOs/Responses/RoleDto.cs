namespace IAM.Domain.DTOs.Responses;

public record RoleDto(
   Guid Id,
   string Name,
   bool IsActive,
   bool IsDefault,
   Guid? OrganizationId);
