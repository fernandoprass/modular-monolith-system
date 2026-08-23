using Myce.Response.Messages;

namespace Shared.Domain.Messages;

public class ParameterDuplicatedError : ErrorMessage
{
   public ParameterDuplicatedError(string module, string group, string name)
      : base(
         SharedTranslatedMessagesProvider.ParameterDuplicatedError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.ParameterDuplicatedError))
   {
      AddVariable(SharedConst.Message.Variable.Module, module);
      AddVariable(SharedConst.Message.Variable.Group, group);
      AddVariable(SharedConst.Message.Variable.Name, name);
   }
}

public class ParameterNotOwnerEditableError : ErrorMessage
{
   public ParameterNotOwnerEditableError()
      : base(
         SharedTranslatedMessagesProvider.ParameterNotOwnerEditableError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.ParameterNotOwnerEditableError)) { }
}

public class ParameterInvalidValueFormatError : ErrorMessage
{
   public ParameterInvalidValueFormatError(string typeName)
      : base(
         SharedTranslatedMessagesProvider.ParameterInvalidValueFormatError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.ParameterInvalidValueFormatError))
   {
      AddVariable(SharedConst.Message.Variable.TypeName, typeName);
   }
}

public class ParameterInvalidValueError(string message)
   : ErrorMessage(SharedTranslatedMessagesProvider.ParameterInvalidValueError, message)
{
}

public class ParameterInvalidKeyFormatError : ErrorMessage
{
   public ParameterInvalidKeyFormatError()
      : base(
         SharedTranslatedMessagesProvider.ParameterInvalidKeyFormatError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.ParameterInvalidKeyFormatError)) { }
}
