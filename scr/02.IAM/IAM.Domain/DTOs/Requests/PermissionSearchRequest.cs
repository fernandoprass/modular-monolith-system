namespace IAM.Domain.DTOs.Requests;

public record PermissionSearchRequest(
   Guid? roleId,
   string? Module,
   string? Group,
   string? Name,
   bool IncludeInactive = false
);
