using Core.API.EndToEnd.Tests.Infrastructure;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;

namespace Core.API.EndToEnd.Tests.Scenarios;

public class RoleEndToEndTests(CoreApiTestFixture fixture) : IClassFixture<CoreApiTestFixture>
{
   private readonly CoreApiTestFixture _fixture = fixture;

   [Fact]
   public async Task RoleFlow_ShouldCreateUpdateAssignPermissionsSearchCreateUserVerifyPermissionsAndDelete()
   {
      var organizationRequest = ScenarioDataFactory.CreateOrganization();
      var organization = await _fixture.Api.CreateOrganizationAsync(
         organizationRequest,
         TestContext.Current.CancellationToken);
      var adminApi = await _fixture.Api.LoginAsync(
         organizationRequest.User.Email,
         organizationRequest.User.Password,
         TestContext.Current.CancellationToken);
      var roleRequest = ScenarioDataFactory.CreateRole(organization.Id);
      var roleUpdateRequest = ScenarioDataFactory.UpdateRoleAsDefault();

      var role = await adminApi.CreateRoleAsync(roleRequest, TestContext.Current.CancellationToken);
      await adminApi.UpdateRoleAsync(role.Id, roleUpdateRequest, TestContext.Current.CancellationToken);

      var userPermissions = await adminApi.SearchPermissionsAsync(
         new PermissionSearchRequest(null, "iam", "users", null),
         TestContext.Current.CancellationToken);
      var viewPermission = GetPermissionByCode(userPermissions, IamPermission.Users.View);
      var updateOrganizationAdminPermission = GetPermissionByCode(userPermissions, IamPermission.Users.UpdateOrganizationAdmin);

      await adminApi.AssignPermissionsAsync(
         new RolePermissionAssignRequest(
            role.Id,
            [viewPermission.Id, updateOrganizationAdminPermission.Id]),
         TestContext.Current.CancellationToken);
      await adminApi.UnassignPermissionsAsync(
         new RolePermissionUnassignRequest(
            role.Id,
            [updateOrganizationAdminPermission.Id]),
         TestContext.Current.CancellationToken);

      var roles = await adminApi.SearchRolesAsync(
         new RoleSearchRequest(roleUpdateRequest.Name, null, true, organization.Id),
         TestContext.Current.CancellationToken);
      var userRequest = ScenarioDataFactory.CreateUser(organization.Id);
      var user = await adminApi.CreateUserAsync(userRequest, TestContext.Current.CancellationToken);
      var userApi = await _fixture.Api.LoginAsync(
         userRequest.Email,
         userRequest.Password,
         TestContext.Current.CancellationToken);

      var hasViewPermission = await userApi.CheckPermissionAsync(
         new PermissionCheckRequest(IamPermission.Users.View),
         TestContext.Current.CancellationToken);
      var hasUpdateOrganizationAdminPermission = await userApi.CheckPermissionAsync(
         new PermissionCheckRequest(IamPermission.Users.UpdateOrganizationAdmin),
         TestContext.Current.CancellationToken);

      var cleanupAdminApi = await _fixture.Api.LoginAsync(
         organizationRequest.User.Email,
         organizationRequest.User.Password,
         TestContext.Current.CancellationToken);
      var adminUser = await cleanupAdminApi.GetCurrentUserAsync(TestContext.Current.CancellationToken);
      var adminPermissions = await cleanupAdminApi.GetUserRolePermissionsAsync(
         adminUser.Id,
         TestContext.Current.CancellationToken);
      Assert.Contains(adminPermissions, permission => permission.Code == IamPermission.Roles.Delete);
      var adminHasRoleDeletePermission = await cleanupAdminApi.CheckPermissionAsync(
         new PermissionCheckRequest(IamPermission.Roles.Delete),
         TestContext.Current.CancellationToken);
      Assert.True(adminHasRoleDeletePermission.Allowed);
      await cleanupAdminApi.DeleteUserAsync(user.Id, TestContext.Current.CancellationToken);
      await cleanupAdminApi.DeleteRoleAsync(role.Id, TestContext.Current.CancellationToken);

      Assert.Contains(roles, searchedRole => searchedRole.Id == role.Id);
      Assert.True(hasViewPermission.Allowed);
      Assert.False(hasUpdateOrganizationAdminPermission.Allowed);
   }

   [Fact]
   public async Task RoleFlow_ShouldAssignRoleDirectlyToUserAndVerifyPermission()
   {
      var organizationRequest = ScenarioDataFactory.CreateOrganization();
      var organization = await _fixture.Api.CreateOrganizationAsync(
         organizationRequest,
         TestContext.Current.CancellationToken);
      var adminApi = await _fixture.Api.LoginAsync(
         organizationRequest.User.Email,
         organizationRequest.User.Password,
         TestContext.Current.CancellationToken);
      var roleRequest = ScenarioDataFactory.CreateRole(organization.Id);
      var userRequest = ScenarioDataFactory.CreateUser(organization.Id);

      var role = await adminApi.CreateRoleAsync(roleRequest, TestContext.Current.CancellationToken);
      var permissions = await adminApi.SearchPermissionsAsync(
         new PermissionSearchRequest(null, "iam", "users", "view"),
         TestContext.Current.CancellationToken);
      var viewPermission = GetPermissionByCode(permissions, IamPermission.Users.View);
      await adminApi.AssignPermissionsAsync(
         new RolePermissionAssignRequest(role.Id, [viewPermission.Id]),
         TestContext.Current.CancellationToken);

      var user = await adminApi.CreateUserAsync(userRequest, TestContext.Current.CancellationToken);
      await adminApi.AssignRoleAsync(
         new RoleAssignRequest(user.Id, [new RoleAssignRoleRequest(role.Id, null)]),
         TestContext.Current.CancellationToken);
      var userApi = await _fixture.Api.LoginAsync(
         userRequest.Email,
         userRequest.Password,
         TestContext.Current.CancellationToken);

      var hasViewPermission = await userApi.CheckPermissionAsync(
         new PermissionCheckRequest(IamPermission.Users.View),
         TestContext.Current.CancellationToken);

      await adminApi.DeleteUserAsync(user.Id, TestContext.Current.CancellationToken);
      await adminApi.DeleteRoleAsync(role.Id, TestContext.Current.CancellationToken);

      Assert.True(hasViewPermission.Allowed);
   }

   private static PermissionDto GetPermissionByCode(
      IReadOnlyCollection<PermissionDto> permissions,
      string code)
   {
      return permissions.First(permission => string.Equals(permission.Code, code, StringComparison.OrdinalIgnoreCase));
   }
}
