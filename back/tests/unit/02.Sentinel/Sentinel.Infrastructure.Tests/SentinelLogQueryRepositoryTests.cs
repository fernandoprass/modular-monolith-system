using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.QueryRepositories;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using System.Reflection;

namespace Sentinel.Infrastructure.Tests;

public class SentinelLogQueryRepositoryTests
{
   public SentinelLogQueryRepositoryTests()
   {
      _ = new SentinelDbContext(new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["ConnectionStrings:SentinelDb"] = "mongodb://localhost:27017",
            ["Sentinel:DatabaseName"] = "sentinel_filter_tests"
         })
         .Build());
   }

   [Fact]
   public void BuildAuditLogFilter_ShouldForceUserOwnerOrganization_WhenUserIsNotSystemAdmin()
   {
      var userOwnerId = Guid.NewGuid();
      var requestedOrganizationId = Guid.NewGuid();
      var userContext = CreateUserContext(isSystemAdmin: false, userOwnerId);
      var request = new AuditLogSearchRequest(
         requestedOrganizationId,
         null,
         "iam",
         null,
         null,
         null,
         null,
         null,
         null);

      var filter = InvokeFilter<AuditLog>("BuildAuditLogFilter", request, userContext);
      var document = Render(filter);
      var json = document.ToJson();

      Assert.Contains("OrganizationId", json);
      Assert.True(ContainsGuidValue(document, userOwnerId));
      Assert.False(ContainsGuidValue(document, requestedOrganizationId));
      Assert.Contains("Module", json);
   }

   [Fact]
   public void BuildAuditLogFilter_ShouldUseRequestedOrganization_WhenUserIsSystemAdmin()
   {
      var requestedOrganizationId = Guid.NewGuid();
      var userContext = CreateUserContext(isSystemAdmin: true, Guid.NewGuid());
      var request = new AuditLogSearchRequest(
         requestedOrganizationId,
         null,
         null,
         null,
         null,
         AuditPrivacyLevel.High,
         null,
         null,
         null);

      var filter = InvokeFilter<AuditLog>("BuildAuditLogFilter", request, userContext);
      var document = Render(filter);
      var json = document.ToJson();

      Assert.Contains("OrganizationId", json);
      Assert.True(ContainsGuidValue(document, requestedOrganizationId));
      Assert.Contains("PrivacyLevel", json);
   }

   [Fact]
   public void BuildSystemLogFilter_ShouldForceUserOwnerOrganization_WhenUserIsNotSystemAdmin()
   {
      var userOwnerId = Guid.NewGuid();
      var requestedOrganizationId = Guid.NewGuid();
      var userContext = CreateUserContext(isSystemAdmin: false, userOwnerId);
      var request = new SystemLogSearchRequest(
         requestedOrganizationId,
         null,
         SystemLogLevel.Error,
         SystemLogStatus.Failure,
         null,
         null,
         null,
         null);

      var filter = InvokeFilter<SystemLog>("BuildSystemLogFilter", request, userContext);
      var document = Render(filter);
      var json = document.ToJson();

      Assert.Contains("OrganizationId", json);
      Assert.True(ContainsGuidValue(document, userOwnerId));
      Assert.False(ContainsGuidValue(document, requestedOrganizationId));
      Assert.Contains("Level", json);
      Assert.Contains("Status", json);
   }

   [Theory]
   [InlineData(0, 0, 1, 50)]
   [InlineData(-1, -1, 1, 50)]
   [InlineData(2, 500, 2, 200)]
   [InlineData(3, 25, 3, 25)]
   public void NormalizePaging_ShouldNormalizeInvalidValuesAndCapPageSize(
      int pageNumber,
      int pageSize,
      int expectedPageNumber,
      int expectedPageSize)
   {
      var method = typeof(SentinelLogQueryRepository).GetMethod("NormalizePaging", BindingFlags.Static | BindingFlags.NonPublic);
      Assert.NotNull(method);

      var result = ((int PageNumber, int PageSize))method.Invoke(null, [pageNumber, pageSize])!;

      Assert.Equal(expectedPageNumber, result.PageNumber);
      Assert.Equal(expectedPageSize, result.PageSize);
   }

   private static IUserContext CreateUserContext(bool isSystemAdmin, Guid userOwnerId)
   {
      var userContext = Substitute.For<IUserContext>();
      userContext.IsSystemAdmin.Returns(isSystemAdmin);
      userContext.UserOwnerId.Returns(userOwnerId);

      return userContext;
   }

   private static FilterDefinition<T> InvokeFilter<T>(string methodName, object request, IUserContext userContext)
   {
      var method = typeof(SentinelLogQueryRepository).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
      Assert.NotNull(method);

      return (FilterDefinition<T>)method.Invoke(null, [request, userContext])!;
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
