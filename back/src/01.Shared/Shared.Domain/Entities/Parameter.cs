using Shared.Domain.Enums;

namespace Shared.Domain.Entities;

public class Parameter : EntityAudited
{
   public string Module { get; private set; }
   public string Group { get; private set; }
   public string Name { get; private set; }
   public string Key { get; private set; }
   public string Title { get; set; }
   public string Description { get; set; }
   public ParameterType Type { get; set; }
   public string Value { get; set; }
   public string? ValidationRegex { get; set; }
   public string? ValidationErrorCustomMessage { get; set; }
   public string? ListItems { get; set; }
   public string? ExternalListEndpoint { get; set; }
   public ParameterOverrideType OverrideType { get; set; }
   public bool IsVisible { get; set; }

   public ICollection<ParameterOverride> ParameterOwners { get; set; } = new List<ParameterOverride>();

   public static Parameter Create(
      string module,
      string group,
      string name,
      string title,
      string description,
      ParameterType type,
      string value,
      string? validationRegex,
      string? validationErrorCustomMessage,
      string? listItems,
      string? externalListEndpoint,
      ParameterOverrideType OverrideType,
      bool isVisible = true)
   {
      var parameter = new Parameter
      {
         Id = Guid.CreateVersion7(),
         Module = module,
         Group = group,
         Name = name,
         Key = GetKey(module, group, name),
         Title = title,
         Description = description,
         Type = type,
         Value = value,
         ValidationRegex = validationRegex,
         ValidationErrorCustomMessage = validationErrorCustomMessage,
         ListItems = listItems,
         ExternalListEndpoint = externalListEndpoint,
         OverrideType = OverrideType,
         IsVisible = isVisible
      };

      return parameter;
   }

   public void Update(
      string module,
      string group,
      string name,
      string title,
      string description,
      ParameterType type,
      string value,
      string? validationRegex,
      string? validationErrorCustomMessage,
      string? listItems,
      string? externalListEndpoint,
      ParameterOverrideType overrideType,
      bool isVisible)
   {
      Module = module;
      Group = group;
      Name = name;
      Key = GetKey(module, group, name);
      Title = title;
      Description = description;
      Type = type;
      Value = value;
      ValidationRegex = validationRegex;
      ValidationErrorCustomMessage = validationErrorCustomMessage;
      ListItems = listItems;
      ExternalListEndpoint = externalListEndpoint;
      OverrideType = overrideType;
      IsVisible = isVisible;
   }

   private static string GetKey(string module, string group, string name)
   {
      return new ParameterKey(module, group, name).Key;
   }
}
