namespace IAM.Domain.DTOs.Requests;

public record RoleSearchRequest(
   string? Name,
   Guid? UserId,
   bool? IsActive,
   Guid? OrganizationId);
