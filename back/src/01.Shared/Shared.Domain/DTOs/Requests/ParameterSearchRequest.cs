namespace Shared.Domain.DTOs.Requests;

public record ParameterSearchRequest(
    string? Module,
    string? Group,
    string? Name,
    string? Key,
    string? Title,
    string? Description
);


public record ParameterSearchRequestInternal(
    string? Module,
    string? Group,
    string? Name,
    string? Key,
    string? Title,
    string? Description,
    Guid UserId,
    Guid OrganizationId,
    bool IsSystemAdmin
);