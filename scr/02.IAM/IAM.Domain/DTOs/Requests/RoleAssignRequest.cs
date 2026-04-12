namespace IAM.Domain.DTOs.Requests;

public record RoleAssignRequest(Guid UserId, IEnumerable<RoleAssignmentDto> Roles);

public record RoleAssignmentDto(Guid Id, DateTime? ExpiresAt);
