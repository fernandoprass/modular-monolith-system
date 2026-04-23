namespace IAM.Domain.DTOs.Requests;

public sealed record OrganizationUpdateRequest
(
    string Name,
    string? Description,
    bool IsActive
);