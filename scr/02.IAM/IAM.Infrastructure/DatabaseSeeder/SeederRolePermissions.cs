using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.QueryRepositories;

namespace IAM.Infrastructure.DatabaseSeeder;

public class SeederRolePermissions(
   IRoleQueryRepository roleQueryRepository,
   IPermissionQueryRepository permissionQueryRepository,
   IPermissionService permissionService)
{
   public async Task SeedAsync(string systemAdminRole, string organizationAdminRole, string userRole)
   {
      var roles = await roleQueryRepository.GetByNameAsync(string.Empty, Guid.Empty);
      var permissions = await permissionQueryRepository.GetByParams(new PermissionSearchRequest(null, null, null));

      var permissionsByCode = permissions.ToDictionary(permission => permission.Code, permission => permission.Id);

      await AssignAsync(roles.FirstOrDefault(role => role.Name == systemAdminRole)?.Id, permissionsByCode, AllPermissions());
      await AssignAsync(roles.FirstOrDefault(role => role.Name == organizationAdminRole)?.Id, permissionsByCode, OrganizationAdminPermissions());
      await AssignAsync(roles.FirstOrDefault(role => role.Name == userRole)?.Id, permissionsByCode, UserPermissions());
   }

   private async Task AssignAsync(
      Guid? roleId,
      IReadOnlyDictionary<string, Guid> permissionsByCode,
      IEnumerable<string> permissionCodes)
   {
      if (roleId is null) return;

      var permissionIds = permissionCodes
         .Where(permissionsByCode.ContainsKey)
         .Select(code => permissionsByCode[code])
         .ToList();

      await permissionService.AssignToRoleAsync(new RolePermissionAssignRequest(roleId.Value, permissionIds));
   }

   private static IEnumerable<string> AllPermissions()
   {
      return OrganizationAdminPermissions();
   }

   private static IEnumerable<string> OrganizationAdminPermissions()
   {
      var permissions = new List<string> 
      {
         IamPermission.Organizations.View,
         IamPermission.Organizations.List,
         IamPermission.Organizations.Create,
         IamPermission.Organizations.Update,
         IamPermission.Organizations.Delete,
         IamPermission.Roles.View,
         IamPermission.Roles.List,
         IamPermission.Roles.Create,
         IamPermission.Roles.Update,
         IamPermission.Roles.Assign,
         IamPermission.Roles.ViewPermissions,
         IamPermission.Parameters.View,
         IamPermission.Parameters.List,
         IamPermission.Parameters.SaveOverride,
         IamPermission.Parameters.DeleteOverride,
         IamPermission.Permissions.List,
         IamPermission.Permissions.Create,
         IamPermission.Permissions.Update,
         IamPermission.Permissions.Delete,
         IamPermission.Permissions.Assign,
         IamPermission.Users.List,
         IamPermission.Users.View,
         IamPermission.Users.Create,
      };

      var userPermissions = UserPermissions();
      permissions.AddRange(userPermissions);

      return permissions;
   }

   private static IEnumerable<string> UserPermissions()
   {
      return
      [
         IamPermission.Users.Update,
         IamPermission.Users.Delete,
      ];
   }
}
