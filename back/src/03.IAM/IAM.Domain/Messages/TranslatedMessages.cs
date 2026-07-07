using Shared.Domain;
using Shared.Domain.Messages;

namespace IAM.Domain.Messages
{
   internal class TranslatedMessages : BaseTranslatedMessages
   {

      public class Organization {

         public const string DuplicateCodeError = nameof(DuplicateCodeError);
         public const string ForbiddenError = nameof(ForbiddenError);
         public const string InvalidCodeFormatError = nameof(InvalidCodeFormatError);
         public const string InvalidTypeError = nameof(InvalidTypeError);
      }

      private void AddTrans()
      {
         AddTranslation(Organization.DuplicateCodeError, LanguageOptions.English, "The organization code '{code}' already exists.");
         AddTranslation(Organization.DuplicateCodeError, LanguageOptions.PortugueseBrazil, "O código de organização '{code}' já existe.");
         AddTranslation(Organization.ForbiddenError, LanguageOptions.English, "The informing organization is different from the logged-in organization.");
         AddTranslation(Organization.ForbiddenError, LanguageOptions.PortugueseBrazil, "A organização informada é diferente da organização do usuário logado.");
         AddTranslation(Organization.InvalidCodeFormatError, LanguageOptions.English, "Code must contain only letters and numbers.");
         AddTranslation(Organization.InvalidCodeFormatError, LanguageOptions.PortugueseBrazil, "O código deve conter apenas letras e números.");
         AddTranslation(Organization.InvalidTypeError, LanguageOptions.English, "Invalid Type, inform 1 for a Company and 2 for an Individual.");
         AddTranslation(Organization.InvalidTypeError, LanguageOptions.PortugueseBrazil, "Tipo inválido. Informe 1 para Pessoa Jurídica e 2 para Pessoa Física.");
      }
   }
}
