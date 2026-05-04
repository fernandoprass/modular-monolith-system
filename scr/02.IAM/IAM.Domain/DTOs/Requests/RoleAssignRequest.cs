namespace IAM.Domain.DTOs.Requests;

public record RoleAssignRequest(Guid UserId, IEnumerable<RoleAssignRoleRequest> Roles);

public record RoleAssignRoleRequest(Guid RoleId, DateTime? ExpiresAt);
