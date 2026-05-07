namespace IAM.Domain.DTOs.Requests;

public record PermissionSearchRequest(
   Guid? roleId,
   string? Module,
   string? Resource,
   string? Action,
   bool IncludeInactive = false
);
