using DatabaseSeeder.Interfaces;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Shared.Infrastructure;

namespace DatabaseSeeder;

public class IamDatabaseSeeder(
   SharedDbContext sharedDbContext,
   IRoleRepository roleRepository,
   IPermissionRepository permissionRepository,
   IOrganizationRepository organizationRepository,
   IIamUnitOfWork iamUnitOfWork) : IDatabaseSeeder
{
   private const string SystemAdminRoleName = "System Admin";
   private const string OrganizationAdminRoleName = "Organization Admin";
   private const string UserRoleName = "User";

   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      await new SeederPermissions(permissionRepository, iamUnitOfWork).SeedAsync(cancellationToken);
      await new SeederRoles(roleRepository, iamUnitOfWork).SeedAsync(SystemAdminRoleName, OrganizationAdminRoleName, UserRoleName, cancellationToken);
      await new SeederRolePermissions(roleRepository, permissionRepository, iamUnitOfWork).SeedAsync(SystemAdminRoleName, OrganizationAdminRoleName, UserRoleName, cancellationToken);
      await new SeederParameters(sharedDbContext).SeedAsync(cancellationToken);
      await new SeederOrganizations(organizationRepository, roleRepository, iamUnitOfWork).SeedAsync(SystemAdminRoleName, OrganizationAdminRoleName, UserRoleName, cancellationToken);
   }
}
