using IAM.Application.Contracts;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;

namespace IAM.Infrastructure.DatabaseSeeder;

public class SeederRoles(IRoleService roleService,
   IIamUnitOfWork iamUnitOfWork)
{
   public async Task SeedAsync(string systemAdminRole, string organizationAdminRole, string userRole)
   {
      var roleSystemAdmin = Role.Create(systemAdminRole,"Full access to all resources.", false, true, null);

      var roleOrganizationAdmin = Role.Create(organizationAdminRole,"Access to all Organization resources and data.", false, true, null);

      var roleUser = Role.Create(userRole, "Access to own resources and data.", true, true, null);

      var roles = await roleService.GetByNameAsync(string.Empty); 
      if (!roles.Data.Any(r => r.Name == systemAdminRole))
      {
         await iamUnitOfWork.Roles.AddAsync(roleSystemAdmin);
      }

      if (!roles.Data.Any(r => r.Name == organizationAdminRole))
      {
         await iamUnitOfWork.Roles.AddAsync(roleOrganizationAdmin);
      }

      if (!roles.Data.Any(r => r.Name == userRole))
      {
         await iamUnitOfWork.Roles.AddAsync(roleUser);
      }

      await iamUnitOfWork.SaveChangesAsync();
   }
}
