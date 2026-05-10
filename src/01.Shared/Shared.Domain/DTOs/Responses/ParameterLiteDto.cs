using Shared.Domain.Enums;

namespace Shared.Domain.DTOs.Responses
{
   public class ParameterLiteDto
   {
      public Guid Id { get; set; }
      public string Module { get; set; }
      public string Group { get; set; }
      public string Name { get; set; }
      public string Key { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public ParameterType Type { get; set; }
      public string Value { get; set; }
      public bool IsOverridden { get; set; }
      public ParameterOverrideType OverrideType { get; set; }
   }
}
