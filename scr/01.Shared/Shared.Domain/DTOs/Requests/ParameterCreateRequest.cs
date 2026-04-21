using Shared.Domain.Enums;

namespace Shared.Domain.DTOs.Requests;

public record ParameterCreateRequest(
    string Module,
    string Group,
    string Name,
    string Title,
    string Description,
    ParameterType Type,
    string Value,
    ParameterOverrideType OverrideType,
    bool IsVisible,
    string? ValidationRegex,
    string? ValidationErrorCustomMessage,
    string? ListItems,
    string? ExternalListEndpoint
)
{
   public ParameterCreateRequest(
      string Module,
      string Group,
      string Name,
      string Title,
      string Description,
      ParameterType Type,
      string Value,
      ParameterOverrideType OverrideType,
      bool IsVisible)
      : this(
           Module,
           Group,
           Name,
           Title,
           Description,
           Type,
           Value,
           OverrideType,
           IsVisible,
           null, null, null, null)
   { }
}
