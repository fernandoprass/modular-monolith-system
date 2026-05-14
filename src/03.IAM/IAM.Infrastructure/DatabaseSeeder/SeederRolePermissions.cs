using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;

namespace IAM.Infrastructure.DatabaseSeeder;

public class SeederRolePermissions(
   IRoleRepository roleRepository,
   IPermissionRepository permissionRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   public async Task SeedAsync(string systemAdminRoleName, string organizationAdminRoleName, string userRoleName)
   {
      var roles = await roleRepository.GetAllByOrganizationAsync(organizationId: null);
      var permissions = await permissionRepository.GetAll(CancellationToken.None);

      var permissionsByCode = permissions.ToDictionary(permission => permission.Code, permission => permission.Id);

      await AssignAsync(roles.FirstOrDefault(role => role.Name == systemAdminRoleName)?.Id, permissionsByCode, AllPermissions());
      await AssignAsync(roles.FirstOrDefault(role => role.Name == organizationAdminRoleName)?.Id, permissionsByCode, OrganizationAdminPermissions());
      await AssignAsync(roles.FirstOrDefault(role => role.Name == userRoleName)?.Id, permissionsByCode, UserPermissions());
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

      var role = await roleRepository.GetByIdAsync(roleId.Value);

      foreach (var permissionId in permissionIds)
      {
         role.AddPermission(permissionId);
      }
      iamUnitOfWork.SaveChangesAsync();
   }

   private static IEnumerable<string> AllPermissions()
   {
      var permissions = OrganizationAdminPermissions().ToList();

      //todo add permissions for audit and system logs
      //permissions.AddRange(SentinelPermission.SystemLogs.List);


      return permissions;
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
         //todo add permissions for audit and system logs
         //SentinelPermission.AuditLogs.List,
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
