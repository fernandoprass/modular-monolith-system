using Shared.Domain;
using Shared.Domain.Messages;

namespace IAM.Domain.Messages
{
   internal sealed class IamTranslatedMessagesProvider : BaseTranslatedMessagesProvider
   {
      public const string OrganizationDuplicateError = nameof(OrganizationDuplicateError);
      public const string OrganizationForbiddenError = nameof(OrganizationForbiddenError);
      public const string OrganizationInvalidCodeFormatError = nameof(OrganizationInvalidCodeFormatError);
      public const string OrganizationInvalidTypeError = nameof(OrganizationInvalidTypeError);
      public const string PermissionDuplicateError = nameof(PermissionDuplicateError);
      public const string PermissionNotFoundInAssignmentError = nameof(PermissionNotFoundInAssignmentError);
      public const string PermissionsCannotBeUnassignedError = nameof(PermissionsCannotBeUnassignedError);
      public const string RoleDuplicateNameError = nameof(RoleDuplicateNameError);
      public const string RolesCannotBeAssignedError = nameof(RolesCannotBeAssignedError);
      public const string RolesCannotBeUnassignedError = nameof(RolesCannotBeUnassignedError);
      public const string RolesInvalidStartDateError = nameof(RolesInvalidStartDateError);
      public const string RolesInvalidExpirationError = nameof(RolesInvalidExpirationError);
      public const string EmailAlreadyExistError = nameof(EmailAlreadyExistError);
      public const string UnauthorizedAccessError = nameof(UnauthorizedAccessError);
      public const string InvalidEmailPasswordError = nameof(InvalidEmailPasswordError);
      public const string AccountLockedError = nameof(AccountLockedError);
      public const string PasswordNotValidError = nameof(PasswordNotValidError);
      public const string PasswordMinLengthError = nameof(PasswordMinLengthError);
      public const string PasswordMissingUppercaseError = nameof(PasswordMissingUppercaseError);
      public const string PasswordMissingLowercaseError = nameof(PasswordMissingLowercaseError);
      public const string PasswordMissingDigitError = nameof(PasswordMissingDigitError);
      public const string PasswordMissingSpecialError = nameof(PasswordMissingSpecialError);

      public static IamTranslatedMessagesProvider Instance { get; } = new();

      private IamTranslatedMessagesProvider()
      {
         AddTranslation(OrganizationDuplicateError, LanguageOptions.English, "The organization code '{code}' already exists.");
         AddTranslation(OrganizationDuplicateError, LanguageOptions.PortugueseBrazil, "O código de organização '{code}' já existe.");
         AddTranslation(OrganizationForbiddenError, LanguageOptions.English, "The informing organization is different from the logged-in organization.");
         AddTranslation(OrganizationForbiddenError, LanguageOptions.PortugueseBrazil, "A organização informada é diferente da organização do usuário logado.");
         AddTranslation(OrganizationInvalidCodeFormatError, LanguageOptions.English, "Code must contain only letters and numbers.");
         AddTranslation(OrganizationInvalidCodeFormatError, LanguageOptions.PortugueseBrazil, "O código deve conter apenas letras e números.");
         AddTranslation(OrganizationInvalidTypeError, LanguageOptions.English, "Invalid Type, inform 1 for a Company and 2 for an Individual.");
         AddTranslation(OrganizationInvalidTypeError, LanguageOptions.PortugueseBrazil, "Tipo inválido. Informe 1 para Pessoa Jurídica e 2 para Pessoa Física.");

         AddTranslation(PermissionDuplicateError, LanguageOptions.English, "The permission code '{code}' already exists.");
         AddTranslation(PermissionDuplicateError, LanguageOptions.PortugueseBrazil, "O código de permissão '{code}' já existe.");
         AddTranslation(PermissionNotFoundInAssignmentError, LanguageOptions.English, "One or more permissions do not exist.");
         AddTranslation(PermissionNotFoundInAssignmentError, LanguageOptions.PortugueseBrazil, "Uma ou mais permissões não existem.");
         AddTranslation(PermissionsCannotBeUnassignedError, LanguageOptions.English, "Permissions cannot be unassigned. One or more permissions are not assigned to the role.");
         AddTranslation(PermissionsCannotBeUnassignedError, LanguageOptions.PortugueseBrazil, "As permissões não podem ser desatribuídas. Uma ou mais permissões não estão atribuídas ao perfil.");

         AddTranslation(RoleDuplicateNameError, LanguageOptions.English, "A role with the name '{name}' already exists.");
         AddTranslation(RoleDuplicateNameError, LanguageOptions.PortugueseBrazil, "Um perfil com o nome '{name}' já existe.");
         AddTranslation(RolesCannotBeAssignedError, LanguageOptions.English, "Roles cannot be assigned. One or more roles added to the list are inactive or belong to another organization.");
         AddTranslation(RolesCannotBeAssignedError, LanguageOptions.PortugueseBrazil, "Os perfis não podem ser atribuídos. Um ou mais perfis adicionados à lista estão inativos ou pertencem a outra organização.");
         AddTranslation(RolesCannotBeUnassignedError, LanguageOptions.English, "Roles cannot be unassigned. One or more roles are not assigned to the user.");
         AddTranslation(RolesCannotBeUnassignedError, LanguageOptions.PortugueseBrazil, "Os perfis não podem ser desatribuídos. Um ou mais perfis não estão atribuídos ao usuário.");
         AddTranslation(RolesInvalidStartDateError, LanguageOptions.English, "Start date should be today or in the future.");
         AddTranslation(RolesInvalidStartDateError, LanguageOptions.PortugueseBrazil, "A data inicial deve ser hoje ou uma data futura.");
         AddTranslation(RolesInvalidExpirationError, LanguageOptions.English, "Expire date should be in the future.");
         AddTranslation(RolesInvalidExpirationError, LanguageOptions.PortugueseBrazil, "A data de expiração deve estar no futuro.");

         AddTranslation(EmailAlreadyExistError, LanguageOptions.English, "The email '{email}' already exists.");
         AddTranslation(EmailAlreadyExistError, LanguageOptions.PortugueseBrazil, "O e-mail '{email}' já existe.");
         AddTranslation(UnauthorizedAccessError, LanguageOptions.English, "Unauthorized access.");
         AddTranslation(UnauthorizedAccessError, LanguageOptions.PortugueseBrazil, "Acesso não autorizado.");
         AddTranslation(InvalidEmailPasswordError, LanguageOptions.English, "Invalid email or password.");
         AddTranslation(InvalidEmailPasswordError, LanguageOptions.PortugueseBrazil, "E-mail ou senha inválidos.");
         AddTranslation(AccountLockedError, LanguageOptions.English, "Account is locked due to too many failed login attempts. Try again in {MinutesRemaining} minute(s).");
         AddTranslation(AccountLockedError, LanguageOptions.PortugueseBrazil, "A conta está bloqueada devido a muitas tentativas de login inválidas. Tente novamente em {MinutesRemaining} minuto(s).");
         AddTranslation(PasswordNotValidError, LanguageOptions.English, "The password is not valid.");
         AddTranslation(PasswordNotValidError, LanguageOptions.PortugueseBrazil, "A senha não é válida.");
         AddTranslation(PasswordMinLengthError, LanguageOptions.English, "Password must contain at least eight letters.");
         AddTranslation(PasswordMinLengthError, LanguageOptions.PortugueseBrazil, "A senha deve conter pelo menos oito caracteres.");
         AddTranslation(PasswordMissingUppercaseError, LanguageOptions.English, "Password must contain at least one uppercase letter.");
         AddTranslation(PasswordMissingUppercaseError, LanguageOptions.PortugueseBrazil, "A senha deve conter pelo menos uma letra maiúscula.");
         AddTranslation(PasswordMissingLowercaseError, LanguageOptions.English, "Password must contain at least one lowercase letter.");
         AddTranslation(PasswordMissingLowercaseError, LanguageOptions.PortugueseBrazil, "A senha deve conter pelo menos uma letra minúscula.");
         AddTranslation(PasswordMissingDigitError, LanguageOptions.English, "Password must contain at least one digit.");
         AddTranslation(PasswordMissingDigitError, LanguageOptions.PortugueseBrazil, "A senha deve conter pelo menos um número.");
         AddTranslation(PasswordMissingSpecialError, LanguageOptions.English, "Password must contain at least one special character (#?!@$%^&*-_.).");
         AddTranslation(PasswordMissingSpecialError, LanguageOptions.PortugueseBrazil, "A senha deve conter pelo menos um caractere especial (#?!@$%^&*-_.).");
      }
   }
}
