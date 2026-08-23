using Courier.Domain.DTOs.Requests;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;
using Courier.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Shared.Domain.Enums;
using System.Reflection;

namespace Courier.Infrastructure.Tests.Repositories;

public class CourierRepositoryFilterTests
{
   public CourierRepositoryFilterTests()
   {
      _ = new CourierDbContext(new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["ConnectionStrings:CourierDb"] = "mongodb://localhost:27017",
            ["Courier:DatabaseName"] = "courier_filter_tests"
         })
         .Build());
   }

   [Fact]
   public void EmailBuildFilter_ShouldIncludeOrganizationAndDateRange()
   {
      var organizationId = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var request = new EmailSearchRequest(
         organizationId,
         userId,
         "iam",
         "users",
         "welcome",
         "person@example.com",
         DateTime.UtcNow.AddDays(-1),
         DateTime.UtcNow);

      var filter = InvokeFilter<Email, EmailSearchRequest>(typeof(EmailRepository), request);
      var document = Render(filter);
      var json = document.ToJson();

      Assert.Contains("OrganizationId", json);
      Assert.Contains("UserId", json);
      Assert.Contains("CreatedAt", json);
      Assert.True(ContainsGuidValue(document, organizationId));
      Assert.True(ContainsGuidValue(document, userId));
      Assert.Contains("Module", json);
      Assert.Contains("Feature", json);
      Assert.Contains("Subject", json);
      Assert.Contains("Recipient", json);
   }

   [Fact]
   public void TemplateBuildFilter_ShouldIncludeModuleKeyAndSeverity()
   {
      var request = new TemplateSearchRequest("iam", "welcome", null, NotificationSeverity.Warning);

      var filter = InvokeFilter<Template, TemplateSearchRequest>(typeof(TemplateRepository), request, "pt-BR");
      var json = Render(filter).ToJson();

      Assert.Contains("module", json);
      Assert.Contains("key", json);
      Assert.Contains("severity", json);
      Assert.Contains("Warning", json);
   }

   [Fact]
   public void NotificationBuildFilter_ShouldIncludeOwnerStatusAndDateRange()
   {
      var organizationId = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var request = new NotificationSearchRequest(
         organizationId,
         userId,
         "iam",
         "welcome",
         NotificationStatus.Unread,
         DateTime.UtcNow.AddDays(-1),
         DateTime.UtcNow);

      var filter = InvokeFilter<Notification, NotificationSearchRequest>(
         typeof(NotificationRepository),
         request);
      var document = Render(filter);
      var json = document.ToJson();

      Assert.Contains("OrganizationId", json);
      Assert.Contains("UserId", json);
      Assert.Contains("CreatedAt", json);
      Assert.Contains("Module", json);
      Assert.Contains("Title", json);
      Assert.Contains("Status", json);
      Assert.True(ContainsGuidValue(document, organizationId));
      Assert.True(ContainsGuidValue(document, userId));
   }

   [Fact]
   public void TemplateBuildFilter_ShouldMatchNameWithinUserLanguageTranslation()
   {
      var request = new TemplateSearchRequest(null, null, "boas-vindas", null);

      var filter = InvokeFilter<Template, TemplateSearchRequest>(typeof(TemplateRepository), request, "pt-br");
      var json = Render(filter).ToJson();

      Assert.Contains("translations", json);
      Assert.Contains("pt-BR", json);
      Assert.Contains("boas-vindas", json);
      Assert.Contains("$elemMatch", json);
   }

   [Fact]
   public void TemplateSerialization_ShouldUseNestedCamelCaseShapeAndStringEnums()
   {
      var template = Template.Create(
         "iam",
         "user-welcome",
         true,
         NotificationSeverity.Information,
         RetentionPolicy.Operational,
         Guid.NewGuid());
      template.AddTranslation(
         TemplateTranslation.Create(
            "pt-br",
            "User welcome",
            TemplateTranslationEmail.Create("Bem-vindo, {{user.name}}", "<p>Sua conta foi criada...</p>"),
            TemplateTranslationNotification.Create("Nova conta criada!", "Acesse seu perfil para começar.", "/perfil")),
         Guid.NewGuid());

      var document = template.ToBsonDocument();
      var translation = document["translations"].AsBsonArray.Single().AsBsonDocument;
      var email = translation["email"].AsBsonDocument;
      var notification = translation["notification"].AsBsonDocument;

      Assert.Equal("iam", document["module"].AsString);
      Assert.Equal("user-welcome", document["key"].AsString);
      Assert.True(document["isAllowingOptOut"].AsBoolean);
      Assert.Equal("Information", document["severity"].AsString);
      Assert.Equal("Operational", document["retentionPolicy"].AsString);
      Assert.Equal("pt-BR", translation["language"].AsString);
      Assert.Equal("User welcome", translation["name"].AsString);
      Assert.Equal("Bem-vindo, {{user.name}}", email["subject"].AsString);
      Assert.True(email["isHtml"].AsBoolean);
      Assert.Equal("Nova conta criada!", notification["title"].AsString);
      Assert.Equal("/perfil", notification["actionLink"].AsString);

      var hydratedTemplate = BsonSerializer.Deserialize<Template>(document);

      Assert.Single(hydratedTemplate.Translations);
      Assert.Equal("pt-BR", hydratedTemplate.Translations.Single().Language);
      Assert.NotNull(hydratedTemplate.Translations.Single().Email);
      Assert.NotNull(hydratedTemplate.Translations.Single().Notification);
   }

   [Fact]
   public void EmailSerialization_ShouldStoreStatusAsString()
   {
      var email = Email.Create(
         Guid.NewGuid(),
         Guid.NewGuid(),
         "iam",
         "users",
         "user-welcome",
         "person@example.com",
         "Welcome",
         "Welcome body",
         false,
         RetentionPolicy.Standard);

      var document = email.ToBsonDocument();

      Assert.Equal("Pending", document["Status"].AsString);

      var hydratedEmail = BsonSerializer.Deserialize<Email>(document);

      Assert.Equal(EmailStatus.Pending, hydratedEmail.Status);
   }

   [Fact]
   public void NotificationSerialization_ShouldStoreStatusAsString()
   {
      var notification = Notification.Create(
         Guid.NewGuid(),
         Guid.NewGuid(),
         "iam",
         "users",
         "user-welcome",
         "Welcome",
         "Your account is ready.",
         "/profile",
         RetentionPolicy.Standard);

      var document = notification.ToBsonDocument();

      Assert.Equal("Unread", document["Status"].AsString);

      var hydratedNotification = BsonSerializer.Deserialize<Notification>(document);

      Assert.Equal(NotificationStatus.Unread, hydratedNotification.Status);
   }

   [Fact]
   public void UserPreferenceSerialization_ShouldHydrateDisabledTemplateCollections()
   {
      var preference = UserPreference.CreateDefault(Guid.NewGuid());
      preference.DisableEmailTemplatePreference("iam", "user-welcome");
      preference.DisableNotificationTemplatePreference("iam", "user-password-updated");

      var document = preference.ToBsonDocument();
      var hydratedPreference = BsonSerializer.Deserialize<UserPreference>(document);

      Assert.Contains(
         hydratedPreference.DisabledEmailTemplates,
         template => template.Module == "iam" && template.TemplateKey == "user-welcome");
      Assert.Contains(
         hydratedPreference.DisabledNotificationTemplates,
         template => template.Module == "iam" && template.TemplateKey == "user-password-updated");
   }

   private static FilterDefinition<TEntity> InvokeFilter<TEntity, TRequest>(
      Type repositoryType,
      TRequest request,
      params object[] additionalArguments)
   {
      var method = repositoryType.GetMethod("BuildFilter", BindingFlags.Static | BindingFlags.NonPublic);
      Assert.NotNull(method);

      return (FilterDefinition<TEntity>)method.Invoke(null, [request!, .. additionalArguments])!;
   }

   private static BsonDocument Render<T>(FilterDefinition<T> filter)
   {
      var registry = BsonSerializer.SerializerRegistry;
      var serializer = registry.GetSerializer<T>();

      return filter.Render(new RenderArgs<T>(serializer, registry));
   }

   private static bool ContainsGuidValue(BsonValue value, Guid expected)
   {
      return value switch
      {
         BsonBinaryData binary => binary.ToGuid(GuidRepresentation.Standard) == expected,
         BsonDocument document => document.Elements.Any(element => ContainsGuidValue(element.Value, expected)),
         BsonArray array => array.Any(item => ContainsGuidValue(item, expected)),
         _ => false
      };
   }
}
