namespace IAM.Domain.DTOs.Requests;

public record PermissionCreateRequest(
   string Module,
   string Resource,
   string Action,
   string Title,
   string Description,
   bool IsActive = true
);
