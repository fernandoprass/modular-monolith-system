using Core.API.EndToEnd.Tests.Infrastructure;

namespace Core.API.EndToEnd.Tests.Scenarios;

public class OrganizationEndToEndTests(CoreApiTestFixture fixture) : IClassFixture<CoreApiTestFixture>
{
   private readonly CoreApiTestFixture _fixture = fixture;

   [Fact]
   public async Task OrganizationFlow_ShouldCreateLoginUpdateVerifyAndDeleteOrganization()
   {
      var createRequest = ScenarioDataFactory.CreateOrganization();
      var updateRequest = ScenarioDataFactory.UpdateOrganization();
      var updateCodeRequest = ScenarioDataFactory.UpdateOrganizationCode();

      var organization = await _fixture.Api.CreateOrganizationAsync(createRequest, TestContext.Current.CancellationToken);
      var authenticatedApi = await _fixture.Api.LoginAsync(
         createRequest.User.Email,
         createRequest.User.Password,
         TestContext.Current.CancellationToken);

      await authenticatedApi.UpdateOrganizationAsync(organization.Id, updateRequest, TestContext.Current.CancellationToken);
      await authenticatedApi.UpdateOrganizationCodeAsync(organization.Id, updateCodeRequest, TestContext.Current.CancellationToken);
      var updatedOrganization = await authenticatedApi.GetOrganizationAsync(organization.Id, TestContext.Current.CancellationToken);
      await authenticatedApi.DeleteOrganizationAsync(organization.Id, TestContext.Current.CancellationToken);

      Assert.Equal(updateRequest.Name, updatedOrganization.Name);
      Assert.Equal(updateRequest.Description, updatedOrganization.Description);
      Assert.Equal(updateRequest.DefaultLanguage, updatedOrganization.DefaultLanguage);
      Assert.Equal(updateRequest.IsActive, updatedOrganization.IsActive);
      Assert.Equal(updateCodeRequest.Code, updatedOrganization.Code);
   }
}
