using Shared.Domain.Enums;

namespace Shared.Domain.DTOs.Responses
{
   public class ParameterValueDto
   {
      public string Key { get; set; }
      public ParameterType Type { get; set; }
      public string Value { get; set; }
   }
}
