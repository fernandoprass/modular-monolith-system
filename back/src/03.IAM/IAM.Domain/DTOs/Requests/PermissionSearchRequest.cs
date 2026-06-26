namespace IAM.Domain.DTOs.Requests;

public record PermissionSearchRequest(
   Guid? roleId,
   string? Module,
   string? Resource,
   string? Action,
   string? Title,
   bool IncludeInactive = false,
   int PageNumber = 1,
   int PageSize = 25
);
