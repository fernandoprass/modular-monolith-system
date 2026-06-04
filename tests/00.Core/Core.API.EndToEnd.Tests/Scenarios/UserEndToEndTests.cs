using Core.API.EndToEnd.Tests.Infrastructure;

namespace Core.API.EndToEnd.Tests.Scenarios;

public class UserEndToEndTests(CoreApiTestFixture fixture) : IClassFixture<CoreApiTestFixture>
{
   private readonly CoreApiTestFixture _fixture = fixture;

   [Fact]
   public async Task UserFlow_ShouldCreateSearchLoginUpdatePasswordVerifyAndDeleteUser()
   {
      var organizationRequest = ScenarioDataFactory.CreateOrganization();
      var organization = await _fixture.Api.CreateOrganizationAsync(
         organizationRequest,
         TestContext.Current.CancellationToken);
      var adminApi = await _fixture.Api.LoginAsync(
         organizationRequest.User.Email,
         organizationRequest.User.Password,
         TestContext.Current.CancellationToken);
      var userRequest = ScenarioDataFactory.CreateUser(organization.Id);
      var userUpdateRequest = ScenarioDataFactory.UpdateUser();
      var passwordUpdateRequest = ScenarioDataFactory.UpdatePassword();

      var user = await adminApi.CreateUserAsync(userRequest, TestContext.Current.CancellationToken);
      var organizationUsers = await adminApi.GetUsersByOrganizationAsync(organization.Id, TestContext.Current.CancellationToken);
      var userApi = await _fixture.Api.LoginAsync(
         userRequest.Email,
         userRequest.Password,
         TestContext.Current.CancellationToken);

      await userApi.UpdateMeAsync(userUpdateRequest, TestContext.Current.CancellationToken);
      await userApi.UpdatePasswordAsync(passwordUpdateRequest, TestContext.Current.CancellationToken);
      var updatedUser = await userApi.GetCurrentUserAsync(TestContext.Current.CancellationToken);
      await adminApi.DeleteUserAsync(user.Id, TestContext.Current.CancellationToken);

      Assert.Contains(organizationUsers, organizationUser => organizationUser.Id == user.Id);
      Assert.Equal(userRequest.Email, user.Email);
      Assert.Equal(userUpdateRequest.Name, updatedUser.Name);
      Assert.Equal(userUpdateRequest.Language, updatedUser.Language);
      Assert.Equal(userUpdateRequest.IsActive, updatedUser.IsActive);
      Assert.Equal(organization.Id, updatedUser.OrganizationId);
   }
}
