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
      await AddPermissionAsync(SentinelPermission.AuditLogs.List, "List Audit Logs", "Allows listing Sentinel audit logs.", cancellationToken);
      await AddPermissionAsync(SentinelPermission.AuditLogs.View, "View Audit Logs", "Allows viewing Sentinel audit logs.", cancellationToken);
      await AddPermissionAsync(SentinelPermission.SystemLogs.List, "List System Logs", "Allows listing Sentinel system logs.", cancellationToken);
      await AddPermissionAsync(SentinelPermission.SystemLogs.View, "View System Logs", "Allows viewing Sentinel system logs.", cancellationToken);
   }

   private async Task AddIamPermissions(CancellationToken cancellationToken)
   {
      Console.WriteLine("Adding IAM permissions...");
      await AddPermissionAsync(IamPermission.Organizations.List, "List Organizations", "Allows listing organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.View, "View Organizations", "Allows viewing organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.ViewOwn, "View Own Organization", "Allows viewing own organization.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.Create, "Create Organizations", "Allows creating organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.Update, "Update Organizations", "Allows updating organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.UpdateOwn, "Update Own Organization", "Allows updating own organization.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.Delete, "Delete Organizations", "Allows deleting organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.DeleteOwn, "Delete Own Organization", "Allows deleting own organization.", cancellationToken);

      await AddPermissionAsync(IamPermission.Users.List, "List Users", "Allows listing users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.View, "View Users", "Allows viewing users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.ViewMe, "View Me", "Allows users to view their own profile.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.Create, "Create Users", "Allows creating users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.UpdateMe, "Update Me", "Allows users to update their own profile.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.Update, "Update Users", "Allows updating users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.UpdateOrganizationAdmin, "Update User Organization Admin", "Allows updating the user organization admin flag.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.UpdatePassword, "Update User Password", "Allows updating user passwords.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.DeleteMe, "Delete Me", "Allows users to delete their own account.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.Delete, "Delete Users", "Allows deleting users.", cancellationToken);

      await AddPermissionAsync(IamPermission.Roles.List, "List Roles", "Allows listing roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.View, "View Roles", "Allows viewing roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Create, "Create Roles", "Allows creating roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Update, "Update Roles", "Allows updating roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Assign, "Assign Roles", "Allows assigning roles to users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.ViewPermissions, "View Role Permissions", "Allows viewing user permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Delete, "Delete Roles", "Allows deleting roles.", cancellationToken);

      await AddPermissionAsync(IamPermission.Parameters.List, "List Parameters", "Allows listing parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.View, "View Parameters", "Allows viewing parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.Update, "Update Parameters", "Allows updating parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.SaveOverride, "Save Parameter Overrides", "Allows saving parameter overrides.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.DeleteOverride, "Delete Parameter Overrides", "Allows deleting parameter overrides.", cancellationToken);

      await AddPermissionAsync(IamPermission.Permissions.List, "List Permissions", "Allows listing permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Update, "Update Permissions", "Allows updating permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Assign, "Assign Permissions", "Allows assigning permissions to roles.", cancellationToken);
   }

   private async Task AddCourierPermissions(CancellationToken cancellationToken)
   {
      Console.WriteLine("Adding Courier permissions...");
      await AddPermissionAsync(CourierPermission.Emails.List, "List Emails", "Allows listing Courier emails.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Emails.View, "View Emails", "Allows viewing Courier emails.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Emails.Create, "Create Emails", "Allows creating Courier emails.", cancellationToken);

      await AddPermissionAsync(CourierPermission.Templates.List, "List Templates", "Allows listing templates.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Templates.View, "View Templates", "Allows viewing templates.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Templates.Create, "Create Templates", "Allows creating templates.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Templates.Update, "Update Templates", "Allows updating templates.", cancellationToken);
      await AddPermissionAsync(CourierPermission.Templates.Delete, "Delete Templates", "Allows deleting templates.", cancellationToken);
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
