using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using Isopoh.Cryptography.Argon2;

namespace IAM.Infrastructure.DatabaseSeeder;

public class SeederOrganizations(
   IOrganizationQueryRepository organizationQueryRepository,
   IRoleQueryRepository roleQueryRepository,
   IIamUnitOfWork iamUnitOfWork)
{
   private const string DefaultPassword = "Password123!";

   public async Task SeedAdminOrgAsync(string systemAdminRole)
   {
      var organizationId = Guid.CreateVersion7();
      var organizationCode = "SAASADMIN";

      if (await organizationQueryRepository.ExistsByCodeAsync(organizationCode)) return;

      var organization = new Organization
      {
         Id = organizationId,
         Name = "SaaS Internal Administration",
         Code = organizationCode,
         Type = OrganizationType.Company,
         Description = "Internal system management and support",
         IsMaster = true, // Following our Master Organization rule
         CreatedAt = DateTime.UtcNow
      };

      var passwordHash = Argon2.Hash(DefaultPassword);

      var superUser = User.Create("System Root", "admin@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), organizationId);
      superUser.IsSystemAdmin = true;
      organization.CreatedBy = superUser.Id;

      var supportUser = User.Create("Internal Support", "support@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), organizationId);
      var roles = await roleQueryRepository.GetAllAsync(systemAdminRole, null);

      superUser.AddRole(roles.First().Id, null);
      supportUser.AddRole(roles.First().Id, null);

      await iamUnitOfWork.Organizations.AddAsync(organization);
      await iamUnitOfWork.Users.AddAsync(superUser);
      await iamUnitOfWork.Users.AddAsync(supportUser);
      await iamUnitOfWork.SaveChangesAsync();
   }

   public async Task SeedScientistsOrgAsync(string organizationAdminRole, string userRole)
   {
      var organizationId = Guid.CreateVersion7();
      var organizationCode = "SCIENTISTS";

      if (await organizationQueryRepository.ExistsByCodeAsync(organizationCode)) return;

      var organization = new Organization
      {
         Id = organizationId,
         Name = "Computing Pioneers Society",
         Code = "SCIENTISTS",
         Type = OrganizationType.Company,
         Description = "Foundation of modern Computer Science",
         CreatedAt = DateTime.UtcNow
      };

      await iamUnitOfWork.Organizations.AddAsync(organization);

      var roles = await roleQueryRepository.GetAllAsync(organizationAdminRole, null);

      var passwordHash = Argon2.Hash(DefaultPassword);
      var alanTuring = User.Create("Alan Turing", "alan.turing@enigma.org", passwordHash, DateTime.UtcNow.AddDays(30), organizationId);
      alanTuring.AddRole(roles.First().Id, null);


      roles = await roleQueryRepository.GetAllAsync(userRole, null);
      var adaLovelace = User.Create("Ada Lovelace", "ada.lovelace@analytical.org", passwordHash, DateTime.UtcNow.AddDays(30), organizationId);
      var graceHopper = User.Create("Grace Hopper", "grace.hopper@cobol.org", passwordHash, DateTime.UtcNow.AddDays(30), organizationId);
      var johnVonNeumann = User.Create("John von Neumann", "john.vonneumann@architecture.org", passwordHash, DateTime.UtcNow.AddDays(30), organizationId);
      var claudeShannon = User.Create("Claude Shannon", "claude.shannon@entropy.org", passwordHash, DateTime.UtcNow.AddDays(30), organizationId);
      adaLovelace.AddRole(roles.First().Id, null);
      graceHopper.AddRole(roles.First().Id, null);
      johnVonNeumann.AddRole(roles.First().Id, null);
      claudeShannon.AddRole(roles.First().Id, null);

      var members = new[]
      {
         alanTuring,
         adaLovelace,
         graceHopper,
         johnVonNeumann,
         claudeShannon
     };

      foreach (var user in members)
      {
         await iamUnitOfWork.Users.AddAsync(user);
      }
      await iamUnitOfWork.SaveChangesAsync();
   }
}
