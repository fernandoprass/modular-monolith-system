using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;

namespace IAM.Infrastructure.DatabaseSeeder;

public class SeederRoles(IRoleRepository roleRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   public async Task SeedAsync(string systemAdminRoleName, string organizationAdminRoleName, string userRoleName)
   {
      var roleSystemAdmin = Role.Create(systemAdminRoleName,"Full access to all resources.", false, true, null);

      var roleOrganizationAdmin = Role.Create(organizationAdminRoleName,"Access to all Organization resources and data.", false, true, null);

      var roleUser = Role.Create(userRoleName, "Access to own resources and data.", true, true, null);

      var roles = await roleRepository.GetAllByOrganizationAsync(organizationId: null); 
      if (!roles.Any(r => r.Name == systemAdminRoleName))
      {
         await iamUnitOfWork.Roles.AddAsync(roleSystemAdmin);
      }

      if (!roles.Any(r => r.Name == organizationAdminRoleName))
      {
         await iamUnitOfWork.Roles.AddAsync(roleOrganizationAdmin);
      }

      if (!roles.Any(r => r.Name == userRoleName))
      {
         await iamUnitOfWork.Roles.AddAsync(roleUser);
      }

      await iamUnitOfWork.SaveChangesAsync();
   }
}
