using DatabaseSeeder.Interfaces;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;

namespace DatabaseSeeder.Authorization;

public class SeederRoles(
   IRoleRepository roleRepository,
   IIamUnitOfWork iamUnitOfWork,
   ISeederData seederData)
{
   public async Task SeedAsync(
      ISeederData seederData,
      CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to add roles...");
      var roles = await roleRepository.GetAllByOrganizationAsync(organizationId: null, cancellationToken);
      var roleNames = roles
         .Where(role => role != null)
         .Select(role => role!.Name)
         .ToHashSet(StringComparer.OrdinalIgnoreCase);

      if (roles != null)
      {
         seederData.SysAdminRoleId = roles!.FirstOrDefault(r => r!.Name.Equals(seederData.SystemAdminRoleName, StringComparison.OrdinalIgnoreCase)).Id;
         seederData.OrganizationAdminRoleId = roles!.FirstOrDefault(r => r!.Name.Equals(seederData.OrganizationAdminRoleName, StringComparison.OrdinalIgnoreCase)).Id;
         seederData.UserRoleId = roles!.FirstOrDefault(r => r!.Name.Equals(seederData.UserRoleName, StringComparison.OrdinalIgnoreCase)).Id;
      }

      if (!roleNames.Contains(seederData.SystemAdminRoleName))
      {
         Console.WriteLine($"Role: {seederData.SystemAdminRoleName}");
         var role = Role.Create(seederData.SystemAdminRoleName, "Full access to all resources.", false, true, null);
         await iamUnitOfWork.Roles.AddAsync(role, cancellationToken);
         seederData.SysAdminRoleId = role.Id;
      }

      if (!roleNames.Contains(seederData.OrganizationAdminRoleName))
      {
         Console.WriteLine($"Role: {seederData.OrganizationAdminRoleName}");
         var role = Role.Create(seederData.OrganizationAdminRoleName, "Access to all Organization resources and data.", false, true, null);
         await iamUnitOfWork.Roles.AddAsync(role, cancellationToken);
         seederData.OrganizationAdminRoleId = role.Id;
      }

      if (!roleNames.Contains(seederData.UserRoleName))
      {
         Console.WriteLine($"Role: {seederData.UserRoleName}");
         var role = Role.Create(seederData.UserRoleName, "Access to own resources and data.", true, true, null);
         await iamUnitOfWork.Roles.AddAsync(role, cancellationToken);
         seederData.UserRoleId = role.Id;
      }

      await iamUnitOfWork.SaveChangesAsync(cancellationToken);

      Console.WriteLine("Finished adding roles...");
      Console.WriteLine();
   }
}
