using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Sentinel.Domain;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure;

public class SentinelDbContext
{
   private readonly IMongoDatabase _database;
   private static int _mongoConfigured;

   public SentinelDbContext(IConfiguration configuration)
   {
      var connectionString = configuration.GetConnectionString(SentinelConst.Database.ConnectionString);

      if (string.IsNullOrWhiteSpace(connectionString))
      {
         throw new InvalidOperationException("Sentinel MongoDB connection string is required.");
      }

      var databaseName = configuration["Sentinel:DatabaseName"];

      ConfigureMongoSerialization();

      var client = new MongoClient(connectionString);

      _database = client.GetDatabase(string.IsNullOrWhiteSpace(databaseName) ? SentinelConst.Database.DefaultName : databaseName);
   }

   public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>(SentinelConst.Database.Collection.AuditLogs);

   public IMongoCollection<SystemLog> SystemLogs => _database.GetCollection<SystemLog>(SentinelConst.Database.Collection.SystemLogs);

   public async Task ConfigureIndexesAsync(CancellationToken cancellationToken = default)
   {
      var auditIndexes = new[]
      {
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Descending(a => a.CreatedAt)),
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Ascending(a => a.Module).Descending(a => a.CreatedAt)),
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Ascending(a => a.TargetId).Descending(a => a.CreatedAt)),
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Ascending(a => a.UserId).Descending(a => a.CreatedAt)),
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "ttl_audit_logs_expires_at" })
      };

      var systemIndexes = new[]
      {
         new CreateIndexModel<SystemLog>(
            Builders<SystemLog>.IndexKeys.Ascending(s => s.OrganizationId).Descending(s => s.CreatedAt)),
         new CreateIndexModel<SystemLog>(
            Builders<SystemLog>.IndexKeys.Ascending(s => s.OrganizationId).Ascending(s => s.Level).Descending(s => s.CreatedAt)),
         new CreateIndexModel<SystemLog>(
            Builders<SystemLog>.IndexKeys.Ascending(s => s.RequestId)),
         new CreateIndexModel<SystemLog>(
            Builders<SystemLog>.IndexKeys.Ascending(s => s.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "ttl_system_logs_expires_at" })

      };

      await AuditLogs.Indexes.CreateManyAsync(auditIndexes, cancellationToken);
      await SystemLogs.Indexes.CreateManyAsync(systemIndexes, cancellationToken);
   }

   private static void ConfigureMongoSerialization()
   {
      if (Interlocked.Exchange(ref _mongoConfigured, 1) == 1)
      {
         return;
      }

      TryRegisterGuidSerializer();
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
