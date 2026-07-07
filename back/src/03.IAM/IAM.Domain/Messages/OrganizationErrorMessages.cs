using Myce.Response.Messages;
using Shared.Domain;

namespace IAM.Domain.Messages;

public class OrganizationErrorMessages : ErrorMessage
{
   private const string Code = "code";

   public const string DuplicateCodeError = nameof(DuplicateCodeError);
   public const string ForbiddenError = nameof(ForbiddenError);
   public const string InvalidCodeFormatError = nameof(InvalidCodeFormatError);
   public const string InvalidTypeError = nameof(InvalidTypeError);

   private static readonly Dictionary<string, Dictionary<string, string>> _localTranslations = new()
   {
      {
         DuplicateCodeError, new()
         {
            { LanguageOptions.English, "The organization code '{code}' already exists." },
            { LanguageOptions.PortugueseBrazil, "O código de organização '{code}' já existe." }
         }
      },
      {
         ForbiddenError, new()
         {
            { LanguageOptions.English, "The informing organization is different from the logged-in organization." },
            { LanguageOptions.PortugueseBrazil, "A organização informada é diferente da organização do usuário logado." }
         }
      },
      {
         InvalidCodeFormatError, new()
         {
            { LanguageOptions.English, "Code must contain only letters and number." },
            { LanguageOptions.PortugueseBrazil, "O código deve conter apenas letras e números" }
         }
      },
      {
         InvalidTypeError, new()
         {
            { LanguageOptions.English, "Invalid Type, inform 1 for a Company and 2 for an Individual." },
            { LanguageOptions.PortugueseBrazil, "Tipo inválido. Informe 1 para Pessoa Jurídica e 2 para Pessoa Física" }
         }
      }
   };

   public static ErrorMessage DuplicateCode(string code)
   {
      var error = new ErrorMessage(DuplicateCodeError, _localTranslations[DuplicateCodeError]);
      error.AddVariable(Code, code);
      return error;
   }

   public static ErrorMessage Forbidden()
   {
      return new ErrorMessage(ForbiddenError, _localTranslations[ForbiddenError]);
   }

   public static ErrorMessage InvalidCodeFormat()
   {
      return new ErrorMessage(InvalidCodeFormatError, _localTranslations[InvalidCodeFormatError]);
   }

   public static ErrorMessage InvalidType()
   {
      return new ErrorMessage(InvalidTypeError, _localTranslations[InvalidTypeError]);
   }
}