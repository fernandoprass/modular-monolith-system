namespace IAM.Domain.DTOs.Requests;

public record PermissionCreateRequest(
   string Module,
   string Group,
   string Name,
   string Title,
   string Description,
   bool IsActive = true
);
