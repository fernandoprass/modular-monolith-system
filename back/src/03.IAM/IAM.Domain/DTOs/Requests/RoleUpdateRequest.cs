namespace IAM.Domain.DTOs.Requests;

public record RoleUpdateRequest(string Name, string Description, bool IsDefault, bool IsActive);
