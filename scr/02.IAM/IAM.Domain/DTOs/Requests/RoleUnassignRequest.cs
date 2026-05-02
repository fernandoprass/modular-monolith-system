namespace IAM.Domain.DTOs.Requests;

public record RoleUnassignRequest(Guid UserId, IEnumerable<Guid> RoleIds);
