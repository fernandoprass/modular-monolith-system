namespace IAM.Domain.DTOs.Responses;

public record RoleDto(
   Guid Id,
   string Name,
   string Description,
   bool IsActive,
   bool IsDefault,
   Guid? OrganizationId);
