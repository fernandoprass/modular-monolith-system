using IAM.Domain;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Sentinel.Domain;

namespace DatabaseSeeder;

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
      await AddPermissionAsync(IamPermission.Organizations.Create, "Create Organizations", "Allows creating organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.Update, "Update Organizations", "Allows updating organizations.", cancellationToken);
      await AddPermissionAsync(IamPermission.Organizations.Delete, "Delete Organizations", "Allows deleting organizations.", cancellationToken);

      await AddPermissionAsync(IamPermission.Users.List, "List Users", "Allows listing users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.View, "View Users", "Allows viewing users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.Create, "Create Users", "Allows creating users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.Update, "Update Users", "Allows updating users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Users.Delete, "Delete Users", "Allows deleting users.", cancellationToken);

      await AddPermissionAsync(IamPermission.Roles.List, "List Roles", "Allows listing roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.View, "View Roles", "Allows viewing roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Create, "Create Roles", "Allows creating roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Update, "Update Roles", "Allows updating roles.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.Assign, "Assign Roles", "Allows assigning roles to users.", cancellationToken);
      await AddPermissionAsync(IamPermission.Roles.ViewPermissions, "View Role Permissions", "Allows viewing user permissions.", cancellationToken);

      await AddPermissionAsync(IamPermission.Parameters.List, "List Parameters", "Allows listing parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.View, "View Parameters", "Allows viewing parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.Update, "Update Parameters", "Allows updating parameters.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.SaveOverride, "Save Parameter Overrides", "Allows saving parameter overrides.", cancellationToken);
      await AddPermissionAsync(IamPermission.Parameters.DeleteOverride, "Delete Parameter Overrides", "Allows deleting parameter overrides.", cancellationToken);

      await AddPermissionAsync(IamPermission.Permissions.List, "List Permissions", "Allows listing permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Create, "Create Permissions", "Allows creating permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Update, "Update Permissions", "Allows updating permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Delete, "Delete Permissions", "Allows deleting permissions.", cancellationToken);
      await AddPermissionAsync(IamPermission.Permissions.Assign, "Assign Permissions", "Allows assigning permissions to roles.", cancellationToken);
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
