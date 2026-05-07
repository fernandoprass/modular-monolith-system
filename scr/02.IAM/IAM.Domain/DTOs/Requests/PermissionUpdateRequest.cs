namespace IAM.Domain.DTOs.Requests;

public record PermissionUpdateRequest(
   string Module,
   string Resource,
   string Action,
   string Title,
   string Description,
   bool IsActive
);
