using Myce.Response.Messages;

namespace Shared.Domain.Messages;
public class ParameterDuplicatedError : ErrorMessage
{
   public ParameterDuplicatedError(string module, string group, string name)
      : base("ParameterDuplicatedError", "A parameter with Module '{module}', Group '{group}' and Name '{name}' already exists.")
   {
      AddVariable(nameof(module), module);
      AddVariable(nameof(group), group);
      AddVariable(nameof(name), name);
   }
}

public class ParameterNotOwnerEditableError : ErrorMessage
{
   public ParameterNotOwnerEditableError()
      : base("ParameterNotOwnerEditableError", "This parameter is not editable by owners.") { }
}

public class ParameterInvalidValueFormatError : ErrorMessage
{
   public ParameterInvalidValueFormatError(string typeName)
      : base("ParameterInvalidValueFormatError", "The value provided is not in a valid format for type '{typeName}'.")
   {
      AddVariable(nameof(typeName), typeName);
   }
}
public class ParameterInvalidValueError(string message) : ErrorMessage("ParameterInvalidValueError", message) { }

public class ParameterInvalidKeyFormatError : ErrorMessage
{
   public ParameterInvalidKeyFormatError()
      : base("ParameterInvalidKeyFormatError", "Invalid Parameter Key format. The key must follow the pattern 'Module.Group.Name', where each segment contains at least 2 alphanumeric characters.") { }
}


