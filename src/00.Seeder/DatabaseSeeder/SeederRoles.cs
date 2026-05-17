using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;

namespace DatabaseSeeder;

public class SeederRoles(
   IRoleRepository roleRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   public async Task SeedAsync(
      string systemAdminRoleName,
      string organizationAdminRoleName,
      string userRoleName,
      CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to add roles...");
      var roles = await roleRepository.GetAllByOrganizationAsync(organizationId: null, cancellationToken);
      var roleNames = roles
         .Where(role => role != null)
         .Select(role => role!.Name)
         .ToHashSet(StringComparer.OrdinalIgnoreCase);

      if (!roleNames.Contains(systemAdminRoleName))
      {
         Console.WriteLine($"Role: {systemAdminRoleName}");
         await iamUnitOfWork.Roles.AddAsync(
            Role.Create(systemAdminRoleName, "Full access to all resources.", false, true, null),
            cancellationToken);
      }

      if (!roleNames.Contains(organizationAdminRoleName))
      {
         Console.WriteLine($"Role: {organizationAdminRoleName}");
         await iamUnitOfWork.Roles.AddAsync(
            Role.Create(organizationAdminRoleName, "Access to all Organization resources and data.", false, true, null),
            cancellationToken);
      }

      if (!roleNames.Contains(userRoleName))
      {
         Console.WriteLine($"Role: {userRoleName}");
         await iamUnitOfWork.Roles.AddAsync(
            Role.Create(userRoleName, "Access to own resources and data.", true, true, null),
            cancellationToken);
      }

      await iamUnitOfWork.SaveChangesAsync(cancellationToken);

      Console.WriteLine("Finished adding roles...");
      Console.WriteLine();
   }
}
