using Microsoft.Extensions.Configuration;
using Sentinel.Infrastructure;
using Sentinel.Infrastructure.QueryRepositories;

namespace Sentinel.Infrastructure.Tests;

public class SentinelLogQueryRepositoryTests
{
   [Fact]
   public void Constructor_WithMongoContext_ShouldCreateRepository()
   {
      var configuration = new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["ConnectionStrings:SentinelDb"] = "mongodb://localhost:27017",
            ["Sentinel:DatabaseName"] = "sentinel_tests"
         })
         .Build();

      var context = new SentinelDbContext(configuration);
      var repository = new SentinelLogQueryRepository(context);

      Assert.NotNull(repository);
   }
}
