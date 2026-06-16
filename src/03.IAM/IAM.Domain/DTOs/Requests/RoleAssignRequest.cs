namespace IAM.Domain.DTOs.Requests;

public record RoleAssignRequest(Guid UserId, DateTime StartsAt, DateTime? ExpiresAt, IEnumerable<Guid> RoleIds);

