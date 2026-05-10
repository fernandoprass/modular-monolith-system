namespace IAM.Domain.DTOs.Responses;

public record PermissionDto(
   Guid Id,
   string Module,
   string Resource,
   string Action,
   string Code,
   string Title,
   string Description,
   bool IsActive);
