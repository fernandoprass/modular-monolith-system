using Courier.Domain;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Courier.Infrastructure;

public class CourierDbContext
{
   private readonly IMongoDatabase _database;
   private static int _mongoConfigured;

   public CourierDbContext(IConfiguration configuration)
   {
      var connectionString = configuration.GetConnectionString(CourierConst.Database.ConnectionString);

      if (string.IsNullOrWhiteSpace(connectionString))
      {
         throw new InvalidOperationException("Courier MongoDB connection string is required.");
      }

      var databaseName = configuration["Courier:DatabaseName"];

      ConfigureMongoSerialization();

      var client = new MongoClient(connectionString);

      _database = client.GetDatabase(string.IsNullOrWhiteSpace(databaseName) ? CourierConst.Database.DefaultName : databaseName);
   }

   public async Task PingAsync(CancellationToken cancellationToken = default)
   {
      await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);
   }

   public IMongoCollection<Email> Emails => _database.GetCollection<Email>(CourierConst.Database.Collection.Emails);

   internal IMongoCollection<Notification> Notifications =>
      _database.GetCollection<Notification>(CourierConst.Database.Collection.Notifications);

   internal IMongoCollection<Template> Templates => _database.GetCollection<Template>(CourierConst.Database.Collection.Templates);

   internal IMongoCollection<UserPreference> UserPreferences =>
      _database.GetCollection<UserPreference>(CourierConst.Database.Collection.UserPreferences);

   public async Task ConfigureIndexesAsync(CancellationToken cancellationToken = default)
   {
      await ConfigureIndexesForEmailAsync(cancellationToken);

      await ConfigureIndexesForNotificationAsync(cancellationToken);

      await ConfigureIndexesForTemplateAsync(cancellationToken);

      await ConfigureIndexesForUserPreferenceAsync(cancellationToken);
   }

   private async Task ConfigureIndexesForUserPreferenceAsync(CancellationToken cancellationToken)
   {
      var preferenceIndexes = new[]
      {
         new CreateIndexModel<UserPreference>(
            Builders<UserPreference>.IndexKeys.Ascending(p => p.UserId),
            new CreateIndexOptions { Unique = true, Name = "ux_user_preferences_user_id" })
      };

      await UserPreferences.Indexes.CreateManyAsync(preferenceIndexes, cancellationToken);
   }

   private async Task ConfigureIndexesForNotificationAsync(CancellationToken cancellationToken)
   {
      var notificationIndexes = new[]
      {
         new CreateIndexModel<Notification>(
            Builders<Notification>.IndexKeys.Ascending(n => n.OrganizationId).Ascending(n => n.UserId)
                                            .Ascending(n => n.Status).Descending(n => n.CreatedAt)),

         new CreateIndexModel<Notification>(
            Builders<Notification>.IndexKeys.Ascending(n => n.UserId).Descending(n => n.CreatedAt)),

         new CreateIndexModel<Notification>(
            Builders<Notification>.IndexKeys.Ascending(n => n.UserId).Ascending(n => n.Status)
                                            .Descending(n => n.CreatedAt)),

         new CreateIndexModel<Notification>(
            Builders<Notification>.IndexKeys.Ascending(n => n.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "ttl_notifications_expires_at" })
      };

      await Notifications.Indexes.CreateManyAsync(notificationIndexes, cancellationToken);
   }

   private async Task ConfigureIndexesForEmailAsync(CancellationToken cancellationToken)
   {
      var emailIndexes = new[]
      {
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.OrganizationId).Descending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.UserId).Descending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.Status).Ascending(e => e.NextAttemptAt).Ascending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "ttl_emails_expires_at" })
      };

      await Emails.Indexes.CreateManyAsync(emailIndexes, cancellationToken);

   }

   private async Task ConfigureIndexesForTemplateAsync(CancellationToken cancellationToken)
   {
      var templateIndexes = new[]
      {
         new CreateIndexModel<Template>(
            Builders<Template>.IndexKeys.Ascending(t => t.Module).Ascending(t => t.Key),
            new CreateIndexOptions { Unique = true, Name = "ux_templates_module_key" })
      };

      await Templates.Indexes.CreateManyAsync(templateIndexes, cancellationToken);
   }

   private static void ConfigureMongoSerialization()
   {
      if (Interlocked.Exchange(ref _mongoConfigured, 1) == 1)
      {
         return;
      }

      TryRegisterGuidSerializer();

      if (!BsonClassMap.IsClassMapRegistered(typeof(Email)))
      {
         BsonClassMap.RegisterClassMap<Email>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(e => e.Status)
               .SetSerializer(new EnumSerializer<EmailStatus>(BsonType.String));
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(Notification)))
      {
         BsonClassMap.RegisterClassMap<Notification>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(n => n.Status)
               .SetSerializer(new EnumSerializer<NotificationStatus>(BsonType.String));
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(Template)))
      {
         BsonClassMap.RegisterClassMap<Template>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(t => t.Module).SetElementName("module");
            cm.MapMember(t => t.Key).SetElementName("key");
            cm.MapMember(t => t.IsAllowingOptOut).SetElementName("isAllowingOptOut");
            cm.MapField("_translations").SetElementName("translations");
            cm.UnmapMember(t => t.Translations);
            cm.MapMember(t => t.Severity)
               .SetElementName("severity")
               .SetSerializer(new EnumSerializer<NotificationSeverity>(BsonType.String));
            cm.MapMember(t => t.RetentionPolicy)
               .SetElementName("retentionPolicy")
               .SetSerializer(new EnumSerializer<Shared.Domain.Enums.RetentionPolicy>(BsonType.String));
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(TemplateTranslation)))
      {
         BsonClassMap.RegisterClassMap<TemplateTranslation>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(t => t.Language).SetElementName("language");
            cm.MapMember(t => t.Name).SetElementName("name");
            cm.MapMember(t => t.Email).SetElementName("email");
            cm.MapMember(t => t.Notification).SetElementName("notification");
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(TemplateTranslationEmail)))
      {
         BsonClassMap.RegisterClassMap<TemplateTranslationEmail>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(t => t.Subject).SetElementName("subject");
            cm.MapMember(t => t.Body).SetElementName("body");
            cm.MapMember(t => t.IsHtml).SetElementName("isHtml");
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(TemplateTranslationNotification)))
      {
         BsonClassMap.RegisterClassMap<TemplateTranslationNotification>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(t => t.Title).SetElementName("title");
            cm.MapMember(t => t.Message).SetElementName("message");
            cm.MapMember(t => t.ActionLink).SetElementName("actionLink");
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(UserPreference)))
      {
         BsonClassMap.RegisterClassMap<UserPreference>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(p => p.UserId).SetElementName("userId");
            cm.MapMember(p => p.IsGlobalEmailEnabled).SetElementName("isGlobalEmailEnabled");
            cm.MapMember(p => p.IsGlobalNotificationEnabled).SetElementName("isGlobalNotificationEnabled");
            cm.MapMember(p => p.CreatedAt).SetElementName("createdAt");
            cm.MapMember(p => p.UpdatedAt).SetElementName("updatedAt");
            cm.MapField("_disabledEmailTemplates").SetElementName("disabledEmailTemplates");
            cm.MapField("_disabledNotificationTemplates").SetElementName("disabledNotificationTemplates");
            cm.UnmapMember(p => p.DisabledEmailTemplates);
            cm.UnmapMember(p => p.DisabledNotificationTemplates);
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(UserPreferenceTemplate)))
      {
         BsonClassMap.RegisterClassMap<UserPreferenceTemplate>(cm =>
         {
            cm.AutoMap();
            cm.MapMember(p => p.Module).SetElementName("module");
            cm.MapMember(p => p.TemplateKey).SetElementName("templateKey");
         });
      }
   }

   private static void TryRegisterGuidSerializer()
   {
      try
      {
         BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
      }
      catch (BsonSerializationException ex) when (ex.Message.Contains("already a serializer registered", StringComparison.OrdinalIgnoreCase))
      {
      }
   }
}
