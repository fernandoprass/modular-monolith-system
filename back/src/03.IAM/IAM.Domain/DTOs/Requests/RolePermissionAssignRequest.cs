namespace IAM.Domain.DTOs.Requests;

public record RolePermissionAssignRequest(
   Guid RoleId,
   IEnumerable<Guid> PermissionIds
);
