using Courier.Domain.DTOs.Requests;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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
   public void TemplateBuildFilter_ShouldIncludeKeyNameAndType()
   {
      var request = new TemplateSearchRequest("welcome", "Welcome", TemplateType.Email);

      var filter = InvokeFilter<Template, TemplateSearchRequest>(typeof(TemplateRepository), request);
      var json = Render(filter).ToJson();

      Assert.Contains("Key", json);
      Assert.Contains("Name", json);
      Assert.Contains("Type", json);
   }

   private static FilterDefinition<TEntity> InvokeFilter<TEntity, TRequest>(Type repositoryType, TRequest request)
   {
      var method = repositoryType.GetMethod("BuildFilter", BindingFlags.Static | BindingFlags.NonPublic);
      Assert.NotNull(method);

      return (FilterDefinition<TEntity>)method.Invoke(null, [request])!;
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
