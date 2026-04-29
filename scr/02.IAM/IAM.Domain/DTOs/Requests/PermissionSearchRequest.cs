namespace IAM.Domain.DTOs.Requests;

public record PermissionSearchRequest(
   string? Module,
   string? Group,
   string? Name
);
