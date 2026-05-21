using Courier.Domain;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Courier.Infrastructure;

public class CourierDbContext
{
   private const string DefaultDatabaseName = "courier";

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

      _database = client.GetDatabase(string.IsNullOrWhiteSpace(databaseName) ? DefaultDatabaseName : databaseName);
   }

   public async Task PingAsync(CancellationToken cancellationToken = default)
   {
      await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);
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
