using Shared.Domain;
using Shared.Domain.Messages;

namespace Courier.Domain.Messages;

public sealed class CourierTranslatedMessagesProvider : BaseTranslatedMessagesProvider
{
   public const string TemplateDuplicateKeyError = nameof(TemplateDuplicateKeyError);
   public const string TemplateTranslationAlreadyExistsError = nameof(TemplateTranslationAlreadyExistsError);
   public const string TemplateTranslationNotFoundError = nameof(TemplateTranslationNotFoundError);
   public const string TemplateChannelRequiredError = nameof(TemplateChannelRequiredError);
   public const string TemplateEmailChannelNotFoundError = nameof(TemplateEmailChannelNotFoundError);
   public const string EmailTemplatePlaceholderMissingError = nameof(EmailTemplatePlaceholderMissingError);
   public const string TemplateLanguageNotFoundError = nameof(TemplateLanguageNotFoundError);

   public static CourierTranslatedMessagesProvider Instance { get; } = new();

   private CourierTranslatedMessagesProvider()
   {
      AddVariableTranslations();

      AddTranslation(TemplateDuplicateKeyError, LanguageOptions.English, "Template key already exists in module {module}: {key}.");
      AddTranslation(TemplateDuplicateKeyError, LanguageOptions.Spanish, "La clave de plantilla ya existe en el módulo {module}: {key}.");
      AddTranslation(TemplateDuplicateKeyError, LanguageOptions.PortugueseBrazil, "A chave do modelo já existe no módulo {module}: {key}.");

      AddTranslation(TemplateTranslationAlreadyExistsError, LanguageOptions.English, "Translation already exists for language: {language}.");
      AddTranslation(TemplateTranslationAlreadyExistsError, LanguageOptions.Spanish, "Ya existe una traducción para el idioma: {language}.");
      AddTranslation(TemplateTranslationAlreadyExistsError, LanguageOptions.PortugueseBrazil, "A tradução já existe para o idioma: {language}.");

      AddTranslation(TemplateTranslationNotFoundError, LanguageOptions.English, "Translation not found for language: {language}.");
      AddTranslation(TemplateTranslationNotFoundError, LanguageOptions.Spanish, "Traducción no encontrada para el idioma: {language}.");
      AddTranslation(TemplateTranslationNotFoundError, LanguageOptions.PortugueseBrazil, "Tradução não encontrada para o idioma: {language}.");

      AddTranslation(TemplateChannelRequiredError, LanguageOptions.English, "At least one template channel is required.");
      AddTranslation(TemplateChannelRequiredError, LanguageOptions.Spanish, "Se requiere al menos un canal de plantilla.");
      AddTranslation(TemplateChannelRequiredError, LanguageOptions.PortugueseBrazil, "Pelo menos um canal de modelo é obrigatório.");

      AddTranslation(TemplateEmailChannelNotFoundError, LanguageOptions.English, "Template {key} does not have email content for language: {language}.");
      AddTranslation(TemplateEmailChannelNotFoundError, LanguageOptions.Spanish, "La plantilla {key} no tiene contenido de correo electrónico para el idioma: {language}.");
      AddTranslation(TemplateEmailChannelNotFoundError, LanguageOptions.PortugueseBrazil, "O modelo {key} não possui conteúdo de e-mail para o idioma: {language}.");

      AddTranslation(EmailTemplatePlaceholderMissingError, LanguageOptions.English, "Email template placeholder is missing: {placeholder}.");
      AddTranslation(EmailTemplatePlaceholderMissingError, LanguageOptions.Spanish, "Falta el marcador de posición de la plantilla de correo electrónico: {placeholder}.");
      AddTranslation(EmailTemplatePlaceholderMissingError, LanguageOptions.PortugueseBrazil, "O marcador de posição do modelo de e-mail está ausente: {placeholder}.");

      AddTranslation(TemplateLanguageNotFoundError, LanguageOptions.English, "Email template {key} does not have translation for language: {language}.");
      AddTranslation(TemplateLanguageNotFoundError, LanguageOptions.Spanish, "La plantilla de correo electrónico {key} no tiene traducción para el idioma: {language}.");
      AddTranslation(TemplateLanguageNotFoundError, LanguageOptions.PortugueseBrazil, "O modelo de e-mail {key} não possui tradução para o idioma: {language}.");
   }
   private void AddVariableTranslations()
   {
      AddVariableTranslation(CourierConst.Message.Variable.Email, LanguageOptions.English, "Email");
      AddVariableTranslation(CourierConst.Message.Variable.Email, LanguageOptions.Spanish, "Correo electrónico");
      AddVariableTranslation(CourierConst.Message.Variable.Email, LanguageOptions.PortugueseBrazil, "E-mail");

      AddVariableTranslation(CourierConst.Message.Variable.Notification, LanguageOptions.English, "Notification");
      AddVariableTranslation(CourierConst.Message.Variable.Notification, LanguageOptions.Spanish, "Notificación");
      AddVariableTranslation(CourierConst.Message.Variable.Notification, LanguageOptions.PortugueseBrazil, "Notificação");

      AddVariableTranslation(CourierConst.Message.Variable.Template, LanguageOptions.English, "Template");
      AddVariableTranslation(CourierConst.Message.Variable.Template, LanguageOptions.Spanish, "Plantilla");
      AddVariableTranslation(CourierConst.Message.Variable.Template, LanguageOptions.PortugueseBrazil, "Modelo");

      AddVariableTranslation(CourierConst.Message.Variable.TemplateTranslation, LanguageOptions.English, "Template translation");
      AddVariableTranslation(CourierConst.Message.Variable.TemplateTranslation, LanguageOptions.Spanish, "Traducción de plantilla");
      AddVariableTranslation(CourierConst.Message.Variable.TemplateTranslation, LanguageOptions.PortugueseBrazil, "Tradução do modelo");

      AddVariableTranslation(CourierConst.Message.Variable.UserPreference, LanguageOptions.English, "User preference");
      AddVariableTranslation(CourierConst.Message.Variable.UserPreference, LanguageOptions.Spanish, "Preferencia de usuario");
      AddVariableTranslation(CourierConst.Message.Variable.UserPreference, LanguageOptions.PortugueseBrazil, "Preferência do usuário");
   }
}
