using Core.API.EndToEnd.Tests.Infrastructure;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;

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
      Assert.Equal(userUpdateRequest.Language.ToLowerInvariant(), updatedUser.Language);
      Assert.Equal(userUpdateRequest.IsActive, updatedUser.IsActive);
      Assert.Equal(organization.Id, updatedUser.OrganizationId);
   }

   [Fact]
   public async Task UserFlow_ShouldCreateOrganizationAdminAndAllowAdminAction()
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
      var roleRequest = ScenarioDataFactory.CreateRole(organization.Id);
      var organizationCodeRequest = ScenarioDataFactory.UpdateOrganizationCode();

      var role = await adminApi.CreateRoleAsync(roleRequest, TestContext.Current.CancellationToken);
      var organizationPermissions = await adminApi.SearchPermissionsAsync(
         new PermissionSearchRequest(null, "iam", "organizations", null),
         TestContext.Current.CancellationToken);
      var viewPermission = organizationPermissions.First(permission => permission.Code == IamPermission.Organizations.View);
      var updatePermission = organizationPermissions.First(permission => permission.Code == IamPermission.Organizations.Update);
      await adminApi.AssignPermissionsAsync(
         new RolePermissionAssignRequest(role.Id, [viewPermission.Id, updatePermission.Id]),
         TestContext.Current.CancellationToken);
      var user = await adminApi.CreateUserAsync(userRequest, TestContext.Current.CancellationToken);
      await adminApi.AssignRoleAsync(
         new RoleAssignRequest(user.Id, [new RoleAssignRoleRequest(role.Id, null)]),
         TestContext.Current.CancellationToken);
      await adminApi.UpdateOrganizationAdminAsync(
         user.Id,
         new UserUpdateOrganizationAdminRequest(true),
         TestContext.Current.CancellationToken);

      var newAdminApi = await _fixture.Api.LoginAsync(
         userRequest.Email,
         userRequest.Password,
         TestContext.Current.CancellationToken);
      var newAdmin = await newAdminApi.GetCurrentUserAsync(TestContext.Current.CancellationToken);
      await newAdminApi.UpdateOrganizationCodeAsync(
         organization.Id,
         organizationCodeRequest,
         TestContext.Current.CancellationToken);
      var updatedOrganization = await newAdminApi.GetOrganizationAsync(
         organization.Id,
         TestContext.Current.CancellationToken);

      await adminApi.DeleteUserAsync(user.Id, TestContext.Current.CancellationToken);
      await adminApi.DeleteRoleAsync(role.Id, TestContext.Current.CancellationToken);

      Assert.True(newAdmin.IsOrganizationAdmin);
      Assert.Equal(organizationCodeRequest.Code, updatedOrganization.Code);
   }
}
