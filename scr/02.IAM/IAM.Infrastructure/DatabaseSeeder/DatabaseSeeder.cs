using IAM.Application.Contracts;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Shared.Application.Contracts;

namespace IAM.Infrastructure.DatabaseSeeder;

public interface IDatabaseSeeder
{
   Task SeedAsync();
}

public class DatabaseSeeder(
   IOrganizationQueryRepository organizationQueryRepository,
   IRoleRepository roleRepository,
   IPermissionRepository permissionRepository,
   IParameterService parameterService,
   IPermissionService permissionService,
   IIamUnitOfWork iamUnitOfWork) : IDatabaseSeeder
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IOrganizationQueryRepository _organizationQueryRepository = organizationQueryRepository;
   private readonly IRoleRepository _roleRepository = roleRepository;
   private readonly IPermissionRepository _permissionRepository = permissionRepository;
   private readonly IParameterService _parameterService = parameterService;
   private readonly IPermissionService _permissionService = permissionService;

   private const string SystemAdminRoleName = "System Admin";
   private const string OrganizationAdminRoleName = "Organization Admins";
   private const string UserRoleName = "User";

   public async Task SeedAsync()
   {
      var seedParameters = new SeederParameters(_parameterService);
      await seedParameters.SeedAsync();

      var seedPermissions = new SeederPermissions(_permissionService);
      await seedPermissions.SeedAsync();

      var seedRoles = new SeederRoles(_roleRepository, _iamUnitOfWork);
      await seedRoles.SeedAsync(SystemAdminRoleName, OrganizationAdminRoleName, UserRoleName);

      var seedRolePermissions = new SeederRolePermissions(_roleRepository, _permissionRepository, _permissionService);
      await seedRolePermissions.SeedAsync(SystemAdminRoleName, OrganizationAdminRoleName, UserRoleName);

      var seedOrganizations = new SeederOrganizations(_organizationQueryRepository, _roleRepository, _iamUnitOfWork);
      await seedOrganizations.SeedAdminOrgAsync(SystemAdminRoleName);
      await seedOrganizations.SeedScientistsOrgAsync(OrganizationAdminRoleName, UserRoleName);
   }  
}
