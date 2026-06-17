namespace IAM.Domain.DTOs.Responses;

public record RolePermissionCodeDto(
   Guid RoleId,
   string Code);
