namespace IAM.Domain.DTOs.Requests;

public sealed record UserUpdateRequest(
   string Name,
   bool IsActive
);
