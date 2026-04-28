using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using Isopoh.Cryptography.Argon2;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Enums;

namespace IAM.Infrastructure.DatabaseSeeder;

public interface IDatabaseSeeder
{
   Task SeedAsync();
}

public class DatabaseSeeder(
   IOrganizationQueryRepository organizationQueryRepository,
   IRoleQueryRepository roleQueryRepository,
   IPermissionQueryRepository permissionQueryRepository,
   IParameterService parameterService,
   IRoleService roleService,
   IPermissionService permissionService,
   IIamUnitOfWork iamUnitOfWork) : IDatabaseSeeder
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IOrganizationQueryRepository _organizationQueryRepository = organizationQueryRepository;
   private readonly IRoleQueryRepository _roleQueryRepository = roleQueryRepository;
   private readonly IPermissionQueryRepository _permissionQueryRepository = permissionQueryRepository;
   private readonly IParameterService _parameterService = parameterService;
   private readonly IRoleService _roleService = roleService;
   private readonly IPermissionService _permissionService = permissionService;

   private const string SystemAdminRole = "System Admin";
   private const string OrganizationAdminRole = "Organization Admins";
   private const string UserRole = "User";

   public async Task SeedAsync()
   {
      var seedParameters = new SeederParameters(_parameterService);
      await seedParameters.SeedAsync();

      var seedPermissions = new SeederPermissions(_permissionService);
      await seedPermissions.SeedAsync();

      var seedRoles = new SeederRoles(_roleService, _iamUnitOfWork);
      await seedRoles.SeedAsync(SystemAdminRole, OrganizationAdminRole, UserRole);

      var seedRolePermissions = new SeederRolePermissions(_roleQueryRepository, _permissionQueryRepository, _permissionService);
      await seedRolePermissions.SeedAsync(SystemAdminRole, OrganizationAdminRole, UserRole);

      var seedOrganizations = new SeederOrganizations(_organizationQueryRepository, _roleQueryRepository, _iamUnitOfWork);
      await seedOrganizations.SeedAdminOrgAsync(SystemAdminRole);
      await seedOrganizations.SeedScientistsOrgAsync(OrganizationAdminRole, UserRole);
   }  
}
