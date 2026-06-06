using Shared.Domain.Enums;

namespace Shared.Domain.DTOs.Responses
{
   public class ParameterValueDto
   {
      public Guid Id { get; set; }
      public Guid? ParameterOverrideId { get; set; }
      public string Key { get; set; }
      public ParameterType Type { get; set; }
      public string Value { get; set; }
      public string DefaultValue { get; set; }
      public bool CanBeOverride { get; set; }
      public bool IsOverride { get; set; }
      public ParameterOverrideType OverrideType { get; set; }
   }
}
