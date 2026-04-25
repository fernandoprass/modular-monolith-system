using Shared.Domain.Enums;

namespace Shared.Domain.DTOs.Responses;

public record ParameterDto(
    Guid Id,
    string Module,
    string Group,
    string Name,
    string Key,
    string Title,
    string Description,
    ParameterType Type,
    string Value,
    string? ListItems,
    string? ExternalListEndpoint,
    ParameterOverrideType OverrideType,
    bool IsVisible
);