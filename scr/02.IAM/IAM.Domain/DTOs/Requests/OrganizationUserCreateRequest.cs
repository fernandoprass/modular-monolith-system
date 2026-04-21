namespace IAM.Domain.DTOs.Requests;

public sealed record OrganizationUserCreateRequest
(
    string Name,
    string Email,
    string Password
);