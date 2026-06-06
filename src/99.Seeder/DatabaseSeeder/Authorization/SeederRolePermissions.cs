using Courier.Domain;
using DatabaseSeeder.Interfaces;
using IAM.Domain;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Sentinel.Domain;

namespace DatabaseSeeder.Authorization;

public class SeederRolePermissions(
   IRoleRepository roleRepository,
   IPermissionRepository permissionRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   public async Task SeedAsync(
      ISeederData seederData,
      CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to assign permissions to roles...");
      var roles = (await roleRepository.GetAllByOrganizationAsync(organizationId: null, cancellationToken))
         .Where(role => role != null)
         .Select(role => role!)
         .ToArray();

      var permissions = await permissionRepository.GetAll(cancellationToken);
      var permissionsByCode = permissions.ToDictionary(permission => permission.Code, permission => permission.Id);

      await AssignAsync(roles.FirstOrDefault(role => role.Name == seederData.SystemAdminRoleName)?.Id, permissionsByCode, SystemAdminPermissions(), cancellationToken);
      await AssignAsync(roles.FirstOrDefault(role => role.Name == seederData.OrganizationAdminRoleName)?.Id, permissionsByCode, OrganizationAdminPermissions(), cancellationToken);
      await AssignAsync(roles.FirstOrDefault(role => role.Name == seederData.UserRoleName)?.Id, permissionsByCode, UserPermissions(), cancellationToken);

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
      sysAdminPermissions.AddRange(GetCourierSystemAdminPermissions());

      return sysAdminPermissions.Distinct().ToList();
   }

   private static List<string> OrganizationAdminPermissions()
   {
      Console.WriteLine("Adding Organization Admin permissions...");
      var orgAdminPermissions = GetIamOrganizationPermissions();
      orgAdminPermissions.AddRange(GetSentinelOrganizationPermissions());
      orgAdminPermissions.AddRange(GetCourierOrganizationPermissions());

      return orgAdminPermissions.Distinct().ToList();
   }

   private static List<string> UserPermissions()
   { 
      Console.WriteLine("Adding User permissions...");
      var userPermissions = GetIamUserPermissions().ToList();
      userPermissions.AddRange(GetSentinelUserPermissions());
      userPermissions.AddRange(GetCourierUserPermissions());

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
      List<string> sysAdminPermissions = [
         IamPermission.Organizations.Create,
         IamPermission.Organizations.List,
         IamPermission.Permissions.Update,
      ];

      sysAdminPermissions.AddRange(GetIamOrganizationPermissions());
      sysAdminPermissions.AddRange(GetIamUserPermissions());

      return sysAdminPermissions;
   }

   private static List<string> GetIamOrganizationPermissions()
   {
      List<string> organizationPermissions =
      [
         IamPermission.Organizations.View,
         IamPermission.Organizations.Update,
         IamPermission.Organizations.Delete,
         IamPermission.Roles.View,
         IamPermission.Roles.List,
         IamPermission.Roles.Create,
         IamPermission.Roles.Update,
         IamPermission.Roles.Assign,
         IamPermission.Roles.ViewPermissions,
         IamPermission.Roles.Delete,
         IamPermission.Parameters.View,
         IamPermission.Parameters.List,
         IamPermission.Parameters.SaveOverride,
         IamPermission.Parameters.DeleteOverride,
         IamPermission.Permissions.List,
         IamPermission.Permissions.Assign,
         IamPermission.Users.List,
         IamPermission.Users.View,
         IamPermission.Users.Create,
         IamPermission.Users.Update,
         IamPermission.Users.Delete,
         IamPermission.Users.UpdateOrganizationAdmin
      ];

      organizationPermissions.AddRange(GetIamUserPermissions());

      return organizationPermissions;
   }

   private static IEnumerable<string> GetIamUserPermissions()
   {
      return
      [
         IamPermission.Users.UpdateMe,
         IamPermission.Users.DeleteMe,
         IamPermission.Users.UpdatePassword
      ];
   }
   #endregion

   #region Courier Permissions
   private static List<string> GetCourierSystemAdminPermissions()
   {
      var courierSystemAdminPermissions = new List<string>
      {
         CourierPermission.Emails.List,
         CourierPermission.Emails.Create,
         CourierPermission.Emails.View,
         CourierPermission.Templates.List,
         CourierPermission.Templates.Create,
         CourierPermission.Templates.View,
         CourierPermission.Templates.Update,
         CourierPermission.Templates.Delete
      };

      courierSystemAdminPermissions.AddRange(GetCourierOrganizationPermissions());

      return courierSystemAdminPermissions;
   }

   private static List<string> GetCourierOrganizationPermissions()
   {
      List<string> courierOrgPermissions =[];

      courierOrgPermissions.AddRange(GetCourierUserPermissions());

      return courierOrgPermissions;
   }

   private static List<string> GetCourierUserPermissions()
   {
      return [];
   }
   #endregion
}
