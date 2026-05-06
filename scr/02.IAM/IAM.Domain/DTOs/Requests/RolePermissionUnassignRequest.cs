namespace IAM.Domain.DTOs.Requests;

public record RolePermissionUnassignRequest(
   Guid RoleId,
   IEnumerable<Guid> PermissionIds
);
