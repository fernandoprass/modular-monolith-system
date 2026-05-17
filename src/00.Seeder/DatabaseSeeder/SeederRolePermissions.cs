using IAM.Domain;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Sentinel.Domain;

namespace DatabaseSeeder;

public class SeederRolePermissions(
   IRoleRepository roleRepository,
   IPermissionRepository permissionRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   public async Task SeedAsync(
      string systemAdminRoleName,
      string organizationAdminRoleName,
      string userRoleName,
      CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to assign permissions to roles...");
      var roles = (await roleRepository.GetAllByOrganizationAsync(organizationId: null, cancellationToken))
         .Where(role => role != null)
         .Select(role => role!)
         .ToArray();

      var permissions = await permissionRepository.GetAll(cancellationToken);
      var permissionsByCode = permissions.ToDictionary(permission => permission.Code, permission => permission.Id);

      await AssignAsync(roles.FirstOrDefault(role => role.Name == systemAdminRoleName)?.Id, permissionsByCode, SystemAdminPermissions(), cancellationToken);
      await AssignAsync(roles.FirstOrDefault(role => role.Name == organizationAdminRoleName)?.Id, permissionsByCode, OrganizationAdminPermissions(), cancellationToken);
      await AssignAsync(roles.FirstOrDefault(role => role.Name == userRoleName)?.Id, permissionsByCode, UserPermissions(), cancellationToken);

      Console.WriteLine("Finished assigning permissions to roles...");
      Console.WriteLine();
   }

   private async Task AssignAsync(
      Guid? roleId,
      IReadOnlyDictionary<string, Guid> permissionsByCode,
      List<string> permissionCodes,
      CancellationToken cancellationToken)
   {
      if (roleId is null) return;

      var role = await roleRepository.GetByIdAsync(roleId.Value, cancellationToken);
      if (role == null) return;

      foreach (var permissionCode in permissionCodes)
      {
         if (permissionsByCode.TryGetValue(permissionCode, out var permissionId))
         {
            role.AddPermission(permissionId);
         }
      }

      iamUnitOfWork.Roles.Update(role);
      await iamUnitOfWork.SaveChangesAsync(cancellationToken);
   }

   private static List<string> SystemAdminPermissions()
   {
      Console.WriteLine("Adding System Admin permissions...");
      var sysAdminPermissions = GetIamSystemAdminPermissions();

      sysAdminPermissions.AddRange(GetSentinelSystemAdminPermissions());

      return sysAdminPermissions.Distinct().ToList();
   }

   private static List<string> OrganizationAdminPermissions()
   {
      Console.WriteLine("Adding Organization Admin permissions...");
      var orgAdminPermissions = GetIamOrganizationPermissions();

      orgAdminPermissions.AddRange(GetSentinelOrganizationPermissions());

      return orgAdminPermissions.Distinct().ToList();
   }

   private static List<string> UserPermissions()
   { 
      Console.WriteLine("Adding User permissions...");
      var userPermissions = GetIamUserPermissions().ToList();
      userPermissions.AddRange(GetSentinelUserPermissions());

      return userPermissions.Distinct().ToList();
   }

   #region Sentinel Permissions
   private static List<string> GetSentinelSystemAdminPermissions()
   {
      var sentinelSytemAdminPermissions = new List<string>
      {
         SentinelPermission.SystemLogs.List,
         SentinelPermission.SystemLogs.View
      };

      sentinelSytemAdminPermissions.AddRange(GetSentinelOrganizationPermissions());

      return sentinelSytemAdminPermissions;
   }

   private static List<string> GetSentinelOrganizationPermissions()
   {
      List<string> sentinelOrgPermissions = 
      [
         SentinelPermission.AuditLogs.List,
         SentinelPermission.AuditLogs.View
      ];

      sentinelOrgPermissions.AddRange(GetSentinelUserPermissions());

      return sentinelOrgPermissions;
   }

   private static List<string> GetSentinelUserPermissions()
   {
      return [];
   }


   #endregion

   #region Iam Permissions
   private static List<string> GetIamSystemAdminPermissions()
   {
      List<string> sysAdminPermissions = [];

      sysAdminPermissions.AddRange(GetIamOrganizationPermissions());
      sysAdminPermissions.AddRange(GetIamUserPermissions());

      return sysAdminPermissions;
   }

   private static List<string> GetIamOrganizationPermissions()
   {
      List<string> organizationPermissions =
      [
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
      ];

      organizationPermissions.AddRange(GetIamUserPermissions());

      return organizationPermissions;
   }

   private static IEnumerable<string> GetIamUserPermissions()
   {
      return
      [
         IamPermission.Users.Update,
         IamPermission.Users.Delete,
      ];
   }
   #endregion
}
