using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;

namespace IAM.Infrastructure.DatabaseSeeder;

public class SeederPermissions(IPermissionService permissionService)
{
   public async Task SeedAsync()
   {
      await AddPermission(IamPermission.Organizations.List, "List Organizations", "Allows listing organizations.");
      await AddPermission(IamPermission.Organizations.View, "View Organizations", "Allows viewing organizations.");
      await AddPermission(IamPermission.Organizations.Create, "Create Organizations", "Allows creating organizations.");
      await AddPermission(IamPermission.Organizations.Update, "Update Organizations", "Allows updating organizations.");
      await AddPermission(IamPermission.Organizations.Delete, "Delete Organizations", "Allows deleting organizations.");

      await AddPermission(IamPermission.Users.List, "List Users", "Allows listing users.");
      await AddPermission(IamPermission.Users.View, "View Users", "Allows viewing users.");
      await AddPermission(IamPermission.Users.Create, "Create Users", "Allows creating users.");
      await AddPermission(IamPermission.Users.Update, "Update Users", "Allows updating users.");
      await AddPermission(IamPermission.Users.Delete, "Delete Users", "Allows deleting users.");

      await AddPermission(IamPermission.Roles.List, "List Roles", "Allows listing roles.");
      await AddPermission(IamPermission.Roles.View, "View Roles", "Allows viewing roles.");
      await AddPermission(IamPermission.Roles.Create, "Create Roles", "Allows creating roles.");
      await AddPermission(IamPermission.Roles.Update, "Update Roles", "Allows updating roles.");
      await AddPermission(IamPermission.Roles.Assign, "Assign Roles", "Allows assigning roles to users.");
      await AddPermission(IamPermission.Roles.ViewPermissions, "View Role Permissions", "Allows viewing user permissions.");

      await AddPermission(IamPermission.Parameters.List, "List Parameters", "Allows listing parameters.");
      await AddPermission(IamPermission.Parameters.View, "View Parameters", "Allows viewing parameters.");
      await AddPermission(IamPermission.Parameters.SaveOverride, "Save Parameter Overrides", "Allows saving parameter overrides.");
      await AddPermission(IamPermission.Parameters.DeleteOverride, "Delete Parameter Overrides", "Allows deleting parameter overrides.");
   }

   private async Task AddPermission(string code, string title, string description)
   {
      var parts = code.Split('.');

      var permission = new PermissionCreateRequest(
         parts[0],
         parts[1],
         parts[2],
         title,
         description);

      await permissionService.CreateAsync(permission);
   }
}
