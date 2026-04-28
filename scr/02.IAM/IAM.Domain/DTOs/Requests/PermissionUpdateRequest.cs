namespace IAM.Domain.DTOs.Requests;

public record PermissionUpdateRequest(
   string Module,
   string Group,
   string Name,
   string Title,
   string Description,
   bool IsActive
);
