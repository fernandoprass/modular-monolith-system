using Courier.Domain;
using IAM.Domain;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Sentinel.Domain;

namespace DatabaseSeeder.Authorization;

public class SeederPermissions(
   IPermissionRepository permissionRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   private IEnumerable<Permission> _existingPermissions;

   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to add permissions...");

      _existingPermissions = await permissionRepository.GetAll(cancellationToken);

      await AddIamPermissions(cancellationToken);

      await AddSentinelPermissions(cancellationToken);

      await AddCourierPermissions(cancellationToken);

      await iamUnitOfWork.SaveChangesAsync(cancellationToken);

      Console.WriteLine("Finished adding permissions...");
      Console.WriteLine();
   }

   private async Task AddSentinelPermissions(CancellationToken cancellationToken)
   {
      Console.WriteLine("Adding Sentinel permissions...");
      await AddPermissionAsync(SentinelPermission.AuditLogs.Read, "Read Audit Logs", "Allows reading Sentinel audit logs.", cancellationToken);
      await AddPermissionAsync(SentinelPermission.SystemLogs.Read, "Read System Logs", "Allows reading Sentinel system logs.", cancellationToken);
   }

   private async Task AddIamPermissions(CancellationToken cancellationToken)
   {
      Console.WriteLine("Adding IAM permissions...");
      await AddPermissionAsync(IamPermission.Organizations.Read, "Read Organizations", "Allows reading organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.Write, "Edit Organizations", "Allows creating, updating, and deleting organizations.", cancellationToken);

      await AddPermissionAsync(IamPermission.OrganizationProfile.Read, "Read Own Organization Profile", "Allows viewing own organization profile.", cancellationToken);
      await AddPermissionAsync(IamPermission.OrganizationProfile.Write, "Update Own Organization Profile", "Allows updating own organization profile.", cancellationToken);
      await AddPermissionAsync(IamPermission.OrganizationProfile.Delete, "Delete Own Organization Profile", "Allows deleting own organization profile.", cancellationToken);

      await AddPermissionAsync(IamPermission.Parameters.Read, "Read Parameters", "Allows reading parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.Write, "Edit Parameters", "Allows updating parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.Override, "Override Parameters", "Allows overriding parameters.", cancellationToken);

      await AddPermissionAsync(IamPermission.Permissions.Read, "Read Permissions", "Allows reading permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Write, "Edit Permissions", "Allows updating permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Assign, "Assign Permissions", "Allows assigning permissions to roles.", cancellationToken);

      await AddPermissionAsync(IamPermission.Roles.Read, "List Roles", "Allows reading roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Write, "Edit Roles", "Allows creating, updating, and deleting roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Assign, "Assign Roles", "Allows assigning roles to users.", cancellationToken);

      await AddPermissionAsync(IamPermission.Users.Read, "Read Users", "Allows reading users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.Write, "Edit Users", "Allows creating, updating, and deleting users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.UpdateOrganizationAdmin, "Update User Organization Admin", "Allows updating the user organization admin flag.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.UpdateSupportUser, "Update Support User", "Allows updating the support user flag.", cancellationToken);


      await AddPermissionAsync(IamPermission.UserProfile.Read, "View Own Profile", "Allows users to view their own profile.", cancellationToken);
      await AddPermissionAsync(IamPermission.UserProfile.Write, "Update Own Profile", "Allows users to update their own profile.", cancellationToken);
      await AddPermissionAsync(IamPermission.UserProfile.Delete, "Delete Own Profile", "Allows users to delete their own profile.", cancellationToken);
      await AddPermissionAsync(IamPermission.UserProfile.ViewAccess, "View Own Roles and Permissions", "Allows users to view their own roles and permissions.", cancellationToken);
   }

   private async Task AddCourierPermissions(CancellationToken cancellationToken)
   {
      Console.WriteLine("Adding Courier permissions...");
      await AddPermissionAsync(CourierPermission.Emails.Read, "Read Emails", "Allows reading Courier emails.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Emails.Write, "Create Emails", "Allows creating Courier emails.", cancellationToken);

      await AddPermissionAsync(CourierPermission.Templates.Read, "Read Templates", "Allows reading templates.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Templates.Write, "Edit Templates", "Allows creating, updating, and deleting templates.", cancellationToken);
   }

   private async Task AddPermissionAsync(string code, string title, string description, CancellationToken cancellationToken)
   {
      if (_existingPermissions.Any(permission => permission.Code.Equals(code, StringComparison.OrdinalIgnoreCase))) return;

      Console.WriteLine($"Permission: {code}");

      var parts = code.Split('.');
      var permission = Permission.Create(parts[0], parts[1], parts[2], title, description, isActive: true);

      await iamUnitOfWork.Permissions.AddAsync(permission, cancellationToken);
   }
}
