using Core.API.EndToEnd.Tests.Infrastructure;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;

namespace Core.API.EndToEnd.Tests.Scenarios;

public class UserEndToEndTests(CoreApiTestFixture fixture) : IClassFixture<CoreApiTestFixture>
{
   private readonly CoreApiTestFixture _fixture = fixture;

   // Scenario: Create a user, update their profile and password, and delete them
   // Given a new organization with a logged-in administrator
   // When the administrator creates a user and lists the organization's users
   // Then the list contains the new user and the creation response has the requested email
   // When the user logs in and updates their profile and password
   // And reads their profile back
   // Then the profile has the updated name, language, and active status
   // And the user still belongs to the same organization
   // And the password update and administrator's user deletion succeed
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

   // Scenario: Promote a user and allow them to update their organization's code
   // Given a new organization with a logged-in administrator
   // And a role with organization-read and organization-write permissions
   // When the administrator creates a user, assigns that role, and promotes the user
   // And the promoted user logs in and reads their profile
   // Then the profile identifies the user as an organization administrator
   // When the promoted user updates the organization's code and reads it back
   // Then the organization has the requested code
   // And deleting the user and role succeeds
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
         new PermissionSearchRequest(null, "iam", "organizations", null, null, false),
         TestContext.Current.CancellationToken);
      var viewPermission = organizationPermissions.First(permission => permission.Code == IamPermission.Organizations.Read);
      var updatePermission = organizationPermissions.First(permission => permission.Code == IamPermission.Organizations.Write);
      await adminApi.AssignPermissionsAsync(
         new RolePermissionAssignRequest(role.Id, [viewPermission.Id, updatePermission.Id]),
         TestContext.Current.CancellationToken);
      var user = await adminApi.CreateUserAsync(userRequest, TestContext.Current.CancellationToken);
      await adminApi.AssignRoleAsync(
         new RoleAssignRequest(user.Id, DateTime.UtcNow, null, [role.Id]),
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
