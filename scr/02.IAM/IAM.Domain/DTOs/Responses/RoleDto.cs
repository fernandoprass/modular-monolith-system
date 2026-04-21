namespace IAM.Domain.DTOs.Responses;

public record RoleDto(
   Guid Id,
   string Name,
   Guid? OrganizationId,
   bool IsDefault,
   IEnumerable<PermissionDto> Features);
