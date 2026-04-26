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
   IParameterService parameterService,
   IRoleService roleService,
   IIamUnitOfWork iamUnitOfWork) : IDatabaseSeeder
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IOrganizationQueryRepository _organizationQueryRepository = organizationQueryRepository;
   private readonly IParameterService _parameterService = parameterService;
   private readonly IRoleService _roleService = roleService;

   public async Task SeedAsync()
   {
      var seedOrganizations = new SeederOrganizations(_organizationQueryRepository, _iamUnitOfWork);
      await seedOrganizations.SeedAdminOrgAsync();
      await seedOrganizations.SeedScientistsOrgAsync();

      var seedParameters = new SeederParameters(_parameterService);
      await seedParameters.SeedAsync();

      var seedRoles = new SeederRoles(_roleService);
   }

   

  
}