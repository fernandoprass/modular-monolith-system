using DatabaseSeeder.Authorization;
using DatabaseSeeder.Interfaces;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;

namespace DatabaseSeeder;

public class SeederAuthorization(
   IRoleRepository roleRepository,
   IPermissionRepository permissionRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   public async Task SeedAsync(
      ISeederData seederData,
      CancellationToken cancellationToken = default)
   {
      await new SeederPermissions(permissionRepository, iamUnitOfWork).SeedAsync(cancellationToken);
      await new SeederRoles(roleRepository, iamUnitOfWork, seederData).SeedAsync(seederData, cancellationToken);
      await new SeederRolePermissions(roleRepository, permissionRepository, iamUnitOfWork).SeedAsync(seederData, cancellationToken);
   }
}
