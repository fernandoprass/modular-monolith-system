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
      var orgAdminPermissions = GetIamOrganizationAdminPermissions();
      orgAdminPermissions.AddRange(GetSentinelOrganizationAdminPermissions());
      orgAdminPermissions.AddRange(GetCourierOrganizationAdminPermissions());

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
         SentinelPermission.SystemLogs.Read
      };

      sentinelSytemAdminPermissions.AddRange(GetSentinelOrganizationAdminPermissions());

      return sentinelSytemAdminPermissions;
   }

   private static List<string> GetSentinelOrganizationAdminPermissions()
   {
      List<string> sentinelOrgPermissions = 
      [
         SentinelPermission.AuditLogs.Read
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
         IamPermission.Organizations.Read,
         IamPermission.Organizations.Write,
         IamPermission.Parameters.Write,
         IamPermission.Permissions.Write
      ];

      sysAdminPermissions.AddRange(GetIamOrganizationAdminPermissions());
      sysAdminPermissions.AddRange(GetIamUserPermissions());

      return sysAdminPermissions;
   }

   private static List<string> GetIamOrganizationAdminPermissions()
   {
      List<string> organizationPermissions =
      [
         IamPermission.OrganizationProfile.Read,
         IamPermission.OrganizationProfile.Write,
         IamPermission.OrganizationProfile.Delete,
         IamPermission.Roles.Read,
         IamPermission.Roles.Write,
         IamPermission.Roles.Assign,
         IamPermission.Parameters.Read,
         IamPermission.Parameters.Override,
         IamPermission.Permissions.Read,
         IamPermission.Permissions.Assign,
         IamPermission.Users.Read,
         IamPermission.Users.Write,
         IamPermission.Users.UpdateOrganizationAdmin
      ];

      organizationPermissions.AddRange(GetIamUserPermissions());

      return organizationPermissions;
   }

   private static IEnumerable<string> GetIamUserPermissions()
   {
      return
      [
         IamPermission.UserProfile.Read,
         IamPermission.UserProfile.Write,
         IamPermission.UserProfile.Delete,
         IamPermission.UserProfile.ViewAccess
      ];
   }
   #endregion

   #region Courier Permissions
   private static List<string> GetCourierSystemAdminPermissions()
   {
      var courierSystemAdminPermissions = new List<string>
      {
         CourierPermission.Emails.Read,
         CourierPermission.Emails.Write,
         CourierPermission.Templates.Read,
         CourierPermission.Templates.Write
      };

      courierSystemAdminPermissions.AddRange(GetCourierOrganizationAdminPermissions());

      return courierSystemAdminPermissions;
   }

   private static List<string> GetCourierOrganizationAdminPermissions()
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
