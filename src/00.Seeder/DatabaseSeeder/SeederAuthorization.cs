using DatabaseSeeder.Authorization;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;

namespace DatabaseSeeder;

public class SeederAuthorization(
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
      await new SeederPermissions(permissionRepository, iamUnitOfWork).SeedAsync(cancellationToken);
      await new SeederRoles(roleRepository, iamUnitOfWork).SeedAsync(systemAdminRoleName, organizationAdminRoleName, userRoleName, cancellationToken);
      await new SeederRolePermissions(roleRepository, permissionRepository, iamUnitOfWork).SeedAsync(systemAdminRoleName, organizationAdminRoleName, userRoleName, cancellationToken);
   }
}
