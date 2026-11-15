using Core.API.EndToEnd.Tests.Infrastructure;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;

namespace Core.API.EndToEnd.Tests.Scenarios;

public class PermissionEndToEndTests(CoreApiTestFixture fixture) : IClassFixture<CoreApiTestFixture>
{
   private readonly CoreApiTestFixture _fixture = fixture;

   // Scenario: Find a known permission using search filters
   // Given a new organization with a logged-in administrator
   // When the administrator searches active permissions for IAM users and the read action
   // Then the results contain the iam.users.read permission
   [Fact]
   public async Task PermissionFlow_ShouldSearchPermissionsAndFindKnownPermission()
   {
      var organizationRequest = ScenarioDataFactory.CreateOrganization();
      await _fixture.Api.CreateOrganizationAsync(
         organizationRequest,
         TestContext.Current.CancellationToken);
      var adminApi = await _fixture.Api.LoginAsync(
         organizationRequest.User.Email,
         organizationRequest.User.Password,
         TestContext.Current.CancellationToken);

      var permissions = await adminApi.SearchPermissionsAsync(
         new PermissionSearchRequest(null, "iam", "users", IamPermission.Users.Read.Split('.')[2], null, false),
         TestContext.Current.CancellationToken);

      Assert.Contains(permissions, permission => permission.Code == IamPermission.Users.Read);
   }
}
