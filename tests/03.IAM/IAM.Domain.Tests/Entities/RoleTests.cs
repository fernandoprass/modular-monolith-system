using FluentAssertions;
using IAM.Domain.Entities;

namespace IAM.Domain.Tests.Entities;

public class RoleTests
{
   [Fact]
   public void AddPermission_ShouldAddPermission_WhenPermissionDoesNotExist()
   {
      var role = CreateRole();
      var permissionId = Guid.NewGuid();

      role.AddPermission(permissionId);

      role.RolePermissions.Should().ContainSingle(permission => permission.PermissionId == permissionId);
   }

   [Fact]
   public void AddPermission_ShouldNotAddDuplicatePermission()
   {
      var role = CreateRole();
      var permissionId = Guid.NewGuid();

      role.AddPermission(permissionId);
      role.AddPermission(permissionId);

      role.RolePermissions.Should().ContainSingle(permission => permission.PermissionId == permissionId);
   }

   [Fact]
   public void RemovePermission_ShouldRemovePermission_WhenPermissionExists()
   {
      var role = CreateRole();
      var permissionId = Guid.NewGuid();
      role.AddPermission(permissionId);

      role.RemovePermission(permissionId);

      role.RolePermissions.Should().BeEmpty();
   }

   [Fact]
   public void RemovePermission_ShouldDoNothing_WhenPermissionDoesNotExist()
   {
      var role = CreateRole();
      var permissionId = Guid.NewGuid();
      role.AddPermission(permissionId);

      role.RemovePermission(Guid.NewGuid());

      role.RolePermissions.Should().ContainSingle(permission => permission.PermissionId == permissionId);
   }

   [Fact]
   public void ClearPermissions_ShouldRemoveAllPermissions()
   {
      var role = CreateRole();
      role.AddPermission(Guid.NewGuid());
      role.AddPermission(Guid.NewGuid());

      role.ClearPermissions();

      role.RolePermissions.Should().BeEmpty();
   }

   private static Role CreateRole()
   {
      return Role.Create("Admin", "Admin role", false, true, Guid.NewGuid());
   }
}
