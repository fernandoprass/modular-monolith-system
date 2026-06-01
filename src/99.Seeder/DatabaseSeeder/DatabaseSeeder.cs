using Courier.Domain.Interfaces.Repositories;
using Courier.Infrastructure;
using DatabaseSeeder.Interfaces;
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
   ISeederData seederData,
   IIamUnitOfWork iamUnitOfWork) : IDatabaseSeeder
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      await new SeederAuthorization(roleRepository, permissionRepository, iamUnitOfWork).SeedAsync( seederData, cancellationToken);
      await new SeederParameters(seederData, sharedDbContext).SeedAsync(cancellationToken);
      await new SeederTemplates(courierDbContext, templateRepository, templateWriteRepository).SeedAsync(cancellationToken);
      await new SeederOrganizations(organizationRepository, roleRepository, iamUnitOfWork, seederData).SeedAsync(cancellationToken);
   }
}
