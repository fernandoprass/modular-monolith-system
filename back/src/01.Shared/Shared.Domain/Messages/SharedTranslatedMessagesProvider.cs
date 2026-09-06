namespace Shared.Domain.Messages;

public sealed class SharedTranslatedMessagesProvider : BaseTranslatedMessagesProvider
{
   public const string FailedToRecordDataError = nameof(FailedToRecordDataError);
   public const string NotFoundError = nameof(NotFoundError);
   public const string NotFoundDetailedError = nameof(NotFoundDetailedError);
   public const string UnauthorizedAccessError = nameof(UnauthorizedAccessError);
   public const string InvalidLanguageError = nameof(InvalidLanguageError);
   public const string ParameterDuplicatedError = nameof(ParameterDuplicatedError);
   public const string ParameterNotOwnerEditableError = nameof(ParameterNotOwnerEditableError);
   public const string ParameterInvalidValueFormatError = nameof(ParameterInvalidValueFormatError);
   public const string ParameterInvalidValueError = nameof(ParameterInvalidValueError);
   public const string ParameterInvalidKeyFormatError = nameof(ParameterInvalidKeyFormatError);
   public const string SuccessInfo = "Success";
   public const string CrudCreatedSuccess = nameof(CrudCreatedSuccess);
   public const string CrudUpdatedSuccess = nameof(CrudUpdatedSuccess);
   public const string CrudDeletedSuccess = nameof(CrudDeletedSuccess);

   public static SharedTranslatedMessagesProvider Instance { get; } = new();

   private SharedTranslatedMessagesProvider()
   {
      AddTranslation(FailedToRecordDataError, LanguageOptions.English, "Failed to record data.");
      AddTranslation(FailedToRecordDataError, LanguageOptions.Spanish, "No se pudieron registrar los datos.");
      AddTranslation(FailedToRecordDataError, LanguageOptions.PortugueseBrazil, "Falha ao registrar os dados.");

      AddTranslation(NotFoundError, LanguageOptions.English, "The requested resource was not found.");
      AddTranslation(NotFoundError, LanguageOptions.Spanish, "No se encontró el recurso solicitado.");
      AddTranslation(NotFoundError, LanguageOptions.PortugueseBrazil, "O recurso solicitado não foi encontrado.");

      AddTranslation(NotFoundDetailedError, LanguageOptions.English, "{entity} not found.");
      AddTranslation(NotFoundDetailedError, LanguageOptions.Spanish, "{entity} no encontrado.");
      AddTranslation(NotFoundDetailedError, LanguageOptions.PortugueseBrazil, "{entity} não encontrado(a).");

      AddTranslation(UnauthorizedAccessError, LanguageOptions.English, "You do not have permission to access this resource.");
      AddTranslation(UnauthorizedAccessError, LanguageOptions.Spanish, "No tiene permiso para acceder a este recurso.");
      AddTranslation(UnauthorizedAccessError, LanguageOptions.PortugueseBrazil, "Você não tem permissão para acessar este recurso.");

      AddTranslation(InvalidLanguageError, LanguageOptions.English, "The language '{language}' is not supported.");
      AddTranslation(InvalidLanguageError, LanguageOptions.Spanish, "El idioma '{language}' no es compatible.");
      AddTranslation(InvalidLanguageError, LanguageOptions.PortugueseBrazil, "O idioma '{language}' não é suportado.");

      AddTranslation(ParameterDuplicatedError, LanguageOptions.English, "A parameter with Module '{module}', Group '{group}' and Name '{name}' already exists.");
      AddTranslation(ParameterDuplicatedError, LanguageOptions.Spanish, "Ya existe un parámetro con Módulo '{module}', Grupo '{group}' y Nombre '{name}'.");
      AddTranslation(ParameterDuplicatedError, LanguageOptions.PortugueseBrazil, "Já existe um parâmetro com Módulo '{module}', Grupo '{group}' e Nome '{name}'.");

      AddTranslation(ParameterNotOwnerEditableError, LanguageOptions.English, "This parameter is not editable by owners.");
      AddTranslation(ParameterNotOwnerEditableError, LanguageOptions.Spanish, "Este parámetro no puede ser editado por propietarios.");
      AddTranslation(ParameterNotOwnerEditableError, LanguageOptions.PortugueseBrazil, "Este parâmetro não pode ser editado por proprietários.");

      AddTranslation(ParameterInvalidValueFormatError, LanguageOptions.English, "The value provided is not in a valid format for type '{typeName}'.");
      AddTranslation(ParameterInvalidValueFormatError, LanguageOptions.Spanish, "El valor informado no tiene un formato válido para el tipo '{typeName}'.");
      AddTranslation(ParameterInvalidValueFormatError, LanguageOptions.PortugueseBrazil, "O valor informado não está em um formato válido para o tipo '{typeName}'.");

      AddTranslation(ParameterInvalidKeyFormatError, LanguageOptions.English, "Invalid Parameter Key format. The key must follow the pattern 'Module.Group.Name', where each segment contains at least 2 alphanumeric characters.");
      AddTranslation(ParameterInvalidKeyFormatError, LanguageOptions.Spanish, "Formato de clave de parámetro inválido. La clave debe seguir el patrón 'Module.Group.Name', donde cada segmento contiene al menos 2 caracteres alfanuméricos.");
      AddTranslation(ParameterInvalidKeyFormatError, LanguageOptions.PortugueseBrazil, "Formato de chave de parâmetro inválido. A chave deve seguir o padrão 'Module.Group.Name', onde cada segmento contém pelo menos 2 caracteres alfanuméricos.");
 
      AddTranslation(CrudCreatedSuccess, LanguageOptions.English, "{entity} created successfully.");
      AddTranslation(CrudCreatedSuccess, LanguageOptions.Spanish, "Operación de creación completada para {entity}.");
      AddTranslation(CrudCreatedSuccess, LanguageOptions.PortugueseBrazil, "{entity} criado(a) com sucesso.");

      AddTranslation(CrudUpdatedSuccess, LanguageOptions.English, "{entity} updated successfully.");
      AddTranslation(CrudUpdatedSuccess, LanguageOptions.Spanish, "Operación de actualización completada para {entity}.");
      AddTranslation(CrudUpdatedSuccess, LanguageOptions.PortugueseBrazil, "{entity} atualizado(a) com sucesso.");

      AddTranslation(CrudDeletedSuccess, LanguageOptions.English, "{entity} deleted successfully.");
      AddTranslation(CrudDeletedSuccess, LanguageOptions.Spanish, "Operación de eliminación completada para {entity}.");
      AddTranslation(CrudDeletedSuccess, LanguageOptions.PortugueseBrazil, "{entity} removido(a) com sucesso.");

      AddTranslation(SuccessInfo, LanguageOptions.English, "Operation completed successfully.");
      AddTranslation(SuccessInfo, LanguageOptions.Spanish, "Operación completada correctamente.");
      AddTranslation(SuccessInfo, LanguageOptions.PortugueseBrazil, "Operação concluída com sucesso.");
   }
}
