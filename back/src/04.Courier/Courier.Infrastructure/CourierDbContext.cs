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

   internal IMongoCollection<Template> Templates => _database.GetCollection<Template>(CourierConst.Database.Collection.Templates);

   public async Task ConfigureIndexesAsync(CancellationToken cancellationToken = default)
   {
      var emailIndexes = new[]
      {
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.OrganizationId).Descending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.OrganizationId).Ascending(e => e.UserId).Descending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.OrganizationId).Ascending(e => e.Module).Ascending(e => e.Feature).Descending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.OrganizationId).Ascending(e => e.Recipient).Descending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.Status).Ascending(e => e.NextAttemptAt).Ascending(e => e.CreatedAt)),
         new CreateIndexModel<Email>(
            Builders<Email>.IndexKeys.Ascending(e => e.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "ttl_emails_expires_at" })
      };

      await Emails.Indexes.CreateManyAsync(emailIndexes, cancellationToken);

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
