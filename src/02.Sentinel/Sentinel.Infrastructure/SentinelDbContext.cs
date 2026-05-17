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
   private const string DefaultDatabaseName = "sentinel";

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

      _database = client.GetDatabase(string.IsNullOrWhiteSpace(databaseName) ? DefaultDatabaseName : databaseName);
   }

   public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("audit_logs");

   public IMongoCollection<SystemLog> SystemLogs => _database.GetCollection<SystemLog>("system_logs");

   public async Task ConfigureIndexesAsync(CancellationToken cancellationToken = default)
   {
      var auditIndexes = new[]
      {
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Descending(a => a.Timestamp)),
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Ascending(a => a.Module).Descending(a => a.Timestamp)),
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Ascending(a => a.TargetId).Descending(a => a.Timestamp)),
         new CreateIndexModel<AuditLog>(
            Builders<AuditLog>.IndexKeys.Ascending(a => a.OrganizationId).Ascending(a => a.UserId).Descending(a => a.Timestamp))
      };

      var systemIndexes = new[]
      {
         new CreateIndexModel<SystemLog>(
            Builders<SystemLog>.IndexKeys.Ascending(s => s.OrganizationId).Descending(s => s.Timestamp)),
         new CreateIndexModel<SystemLog>(
            Builders<SystemLog>.IndexKeys.Ascending(s => s.OrganizationId).Ascending(s => s.Level).Descending(s => s.Timestamp)),
         new CreateIndexModel<SystemLog>(
            Builders<SystemLog>.IndexKeys.Ascending(s => s.RequestId))
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

      BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
   }

}
