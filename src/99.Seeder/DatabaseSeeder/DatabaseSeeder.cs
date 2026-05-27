using DatabaseSeeder.Authorization;
using DatabaseSeeder.Interfaces;
using Courier.Domain.Interfaces.Repositories;
using Courier.Infrastructure;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Shared.Infrastructure;

namespace DatabaseSeeder;

public class DatabaseSeeder(
   SharedDbContext sharedDbContext,
   IRoleRepository roleRepository,
   IPermissionRepository permissionRepository,
   IOrganizationRepository organizationRepository,
   CourierDbContext courierDbContext,
   ITemplateRepository templateRepository,
   ITemplateWriteRepository templateWriteRepository,
   IIamUnitOfWork iamUnitOfWork) : IDatabaseSeeder
{
   private const string SystemAdminRoleName = "System Admin";
   private const string OrganizationAdminRoleName = "Organization Admin";
   private const string UserRoleName = "User";

   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      await new SeederAuthorization(roleRepository, permissionRepository, iamUnitOfWork).SeedAsync(SystemAdminRoleName, OrganizationAdminRoleName, UserRoleName, cancellationToken);
      await new SeederParameters(sharedDbContext).SeedAsync(cancellationToken);
      await new SeederTemplates(courierDbContext, templateRepository, templateWriteRepository).SeedAsync(cancellationToken);
      await new SeederOrganizations(organizationRepository, roleRepository, iamUnitOfWork).SeedAsync(SystemAdminRoleName, OrganizationAdminRoleName, UserRoleName, cancellationToken);
   }
}
