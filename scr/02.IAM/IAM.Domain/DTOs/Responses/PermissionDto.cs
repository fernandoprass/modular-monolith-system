namespace IAM.Domain.DTOs.Responses;

public record PermissionDto(
   Guid Id,
   string Module,
   string Group,
   string Name,
   string Code,
   string Title,
   string Description,
   bool IsActive);
