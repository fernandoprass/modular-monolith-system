namespace IAM.Domain.DTOs.Requests;

public sealed record UserCreateRequest(
    string Name,
    string Email,
    string Password,
    string Language,
    Guid OrganizationId
);