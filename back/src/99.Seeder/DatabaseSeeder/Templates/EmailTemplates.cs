using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.ValueObjects;
using Shared.Domain;
using Shared.Domain.Enums;

namespace DatabaseSeeder.Templates;

public class EmailTemplates(
   ITemplateRepository templateRepository,
   ITemplateWriteRepository templateWriteRepository)
{
   private const string Module = "iam";

   private static readonly EmailTemplateSeed[] Templates =
   [
      new(
         "orgazination-welcome",
         "Organization welcome",
         "Welcome to {{organization.name}}.",
         "<p>Welcome to {{organization.name}}.</p>",
         "Bem-vindo a {{organization.name}}.",
         "<p>Bem-vindo a {{organization.name}}.</p>"),
      new(
         "orgazination-delete",
         "Organization delete",
         "{{organization.name}} was deleted.",
         "<p>{{organization.name}} was deleted.</p>",
         "{{organization.name}} foi removida.",
         "<p>{{organization.name}} foi removida.</p>"),
      new(
         "user-welcome",
         "User welcome",
         "Welcome, {{user.name}}.",
         "<p>Welcome, {{user.name}}.</p>",
         "Bem-vindo, {{user.name}}.",
         "<p>Bem-vindo, {{user.name}}.</p>"),
      new(
         "user-reset-password",
         "User reset password",
         "Use the reset password link to continue.",
         "<p>Use the reset password link to continue.</p>",
         "Use o link de redefinicao de senha para continuar.",
         "<p>Use o link de redefinicao de senha para continuar.</p>"),
      new(
         "user-password-updated",
         "User password updated",
         "Your password has been updated.",
         "<p>{{user.name}}</p><p>Your password has been updated.</p>",
         "Sua senha foi atualizada.",
         "<p>{{user.name}}</p><p>Sua senha foi atualizada.</p>"),
      new(
         "user-max-failed-login-attempts",
         "User max failed login attempts",
         "Your account was locked after too many failed login attempts.",
         "<p>Your account was locked after too many failed login attempts.</p>",
         "Sua conta foi bloqueada apos muitas tentativas de login sem sucesso.",
         "<p>Sua conta foi bloqueada apos muitas tentativas de login sem sucesso.</p>"),
      new(
         "user-delete",
         "User delete",
         "The user {{user.email}} was deleted.",
         "<p>The user {{user.email}} was deleted.</p>",
         "O usuario {{user.email}} foi removido.",
         "<p>O usuario {{user.email}} foi removido.</p>")
   ];

   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      foreach (var seed in Templates)
      {
         await AddTemplateAsync(seed, cancellationToken);
      }
   }

   private async Task AddTemplateAsync(EmailTemplateSeed seed, CancellationToken cancellationToken)
   {
      var template = await templateRepository.GetByModuleAndKeyAsync(Module, seed.Key, cancellationToken);

      if (template == null)
      {
         template = Template.Create(
            Module,
            seed.Key,
            false,
            NotificationSeverity.Information,
            RetentionPolicy.Standard,
            Guid.Empty);
         AddTranslation(template, LanguageOptions.English, seed.Name, seed.SubjectEn, seed.BodyEn);
         AddTranslation(template, LanguageOptions.PortugueseBrazil, seed.Name, seed.SubjectPt, seed.BodyPt);

         await templateWriteRepository.AddAsync(template, cancellationToken);
         Console.WriteLine($"Template: {seed.Key}");
         return;
      }

      var changed = false;

      changed |= AddTranslation(template, LanguageOptions.English, seed.Name, seed.SubjectEn, seed.BodyEn);
      changed |= AddTranslation(template, LanguageOptions.PortugueseBrazil, seed.Name, seed.SubjectPt, seed.BodyPt);

      if (changed)
      {
         await templateWriteRepository.UpdateAsync(template, cancellationToken);
         Console.WriteLine($"Template updated: {seed.Key}");
      }
   }

   private static bool AddTranslation(
      Template template,
      string language,
      string name,
      string subject,
      string body)
   {
      var translation = TemplateTranslation.Create(
         language,
         name,
         TemplateTranslationEmail.Create(subject, body),
         null);

      return template.AddTranslation(translation, Guid.Empty);
   }

   private sealed record EmailTemplateSeed(
      string Key,
      string Name,
      string SubjectEn,
      string BodyEn,
      string SubjectPt,
      string BodyPt);
}
