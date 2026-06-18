using Shared.Domain.Enums;

namespace Shared.Domain.DTOs.Requests;

public record ParameterUpdateRequest(
    string Module,
    string Group,
    string Name,
    string Title,
    string Description,
    ParameterType Type,
    string Value,
    ParameterOverrideType OverrideType,
    bool IsVisible,
    string? ValidationRegex = null,        // Default values
    string? ValidationErrorCustomMessage = null,
    string? ListItems = null,
    string? ExternalListEndpoint = null
);
