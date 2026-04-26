using IAM.Application.Contracts;
using IAM.Domain.DTOs.Requests;

namespace IAM.Infrastructure.DatabaseSeeder
{
   public class SeederRoles(IRoleService roleService)
   {
      public async Task SeedAsync()
      {
         var roleSystemAdmin = new RoleCreateRequest
         (
            Name: "System Admin",
            Description: "Full access to all resources.",
            IsDefault: false,
            IsActive: true,
            OrganizationId: null
         );

         var roleOrganizationAdmin = new RoleCreateRequest
         (
            Name: "Organization Admins",
            Description: "Access to all Organization resources and data.",
            IsDefault: false,
            IsActive: true,
            OrganizationId: null
         );

         var roleUser = new RoleCreateRequest
         (
            Name: "User",
            Description: "Access to own resources and data.",
            IsDefault: true,
            IsActive: true,
            OrganizationId: null
         );

         await roleService.CreateAsync(roleSystemAdmin);
         await roleService.CreateAsync(roleOrganizationAdmin);
         await roleService.CreateAsync(roleUser);
      }
}
