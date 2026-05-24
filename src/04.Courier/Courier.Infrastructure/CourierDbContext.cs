using Courier.Domain;
using Courier.Domain.Entities;
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
            Builders<Template>.IndexKeys.Ascending(t => t.Key),
            new CreateIndexOptions { Unique = true, Name = "ux_templates_key" })
      };

      await Templates.Indexes.CreateManyAsync(templateIndexes, cancellationToken);
   }

   private static void ConfigureMongoSerialization()
   {
      if (Interlocked.Exchange(ref _mongoConfigured, 1) == 1)
      {
         return;
      }

      BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

      if (!BsonClassMap.IsClassMapRegistered(typeof(Template)))
      {
         BsonClassMap.RegisterClassMap<Template>(cm =>
         {
            cm.AutoMap();
            cm.MapField("_emailTranslations").SetElementName("emailTranslations");
            cm.UnmapMember(t => t.EmailTranslations);
         });
      }

      if (!BsonClassMap.IsClassMapRegistered(typeof(TemplateEmailTranslation)))
      {
         BsonClassMap.RegisterClassMap<TemplateEmailTranslation>(cm =>
         {
            cm.AutoMap();
         });
      }
   }
}
