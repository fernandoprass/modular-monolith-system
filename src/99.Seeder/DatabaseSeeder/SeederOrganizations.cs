using DatabaseSeeder.Interfaces;
using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Isopoh.Cryptography.Argon2;

namespace DatabaseSeeder;

public class SeederOrganizations(
   IOrganizationRepository organizationRepository,
   IRoleRepository roleRepository,
   IIamUnitOfWork iamUnitOfWork,
   ISeederData seederData)
{
   private const string DefaultPassword = "Password123!";

   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to add organizations...");
      var roles = (await roleRepository.GetAllByOrganizationAsync(null, cancellationToken))
         .Where(role => role != null)
         .Select(role => role!)
         .ToArray();

      await SeedAdminOrgAsync(seederData.SystemAdminRoleName, roles, cancellationToken);
      await SeedScientistsOrgAsync(seederData.OrganizationAdminRoleName, seederData.UserRoleName, roles, cancellationToken);

      Console.WriteLine("Finished adding organizations...");
      Console.WriteLine();
   }

   private async Task SeedAdminOrgAsync(
      string systemAdminRoleName,
      IReadOnlyCollection<Role> roles,
      CancellationToken cancellationToken)
   {
      const string organizationCode = "SAASADMIN";

      var existingOrganization = await organizationRepository.GetByCodeAsync(organizationCode, cancellationToken);

      if (existingOrganization != null) 
      { 
         seederData.SaaSOrganizationId = existingOrganization.Id;
         return;
      } 

      var organization = Organization.Create(
         OrganizationType.Company,
         organizationCode,
         "SaaS Internal Administration",
         "Internal system management and support");

      seederData.SaaSOrganizationId= organization.Id;

      var passwordHash = Argon2.Hash(DefaultPassword);
      var systemRoleId = roles.First(role => role.Name == systemAdminRoleName).Id;

      var superUser = User.Create("System Root", "admin@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), organization.Id);
      superUser.IsSystemAdmin = true;

      superUser.AddRole(systemRoleId, null);
      organization.CreatedBy = superUser.Id;

      var supportUser = User.Create("Internal Support", "support@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), organization.Id);
      supportUser.AddRole(systemRoleId, null);

      Console.WriteLine($"Adding organization: {organization.Name}");
      await iamUnitOfWork.Organizations.AddAsync(organization, cancellationToken);

      Console.WriteLine($"Adding users for organization: {organization.Name}");
      Console.WriteLine($" - {superUser.Email} (System Admin)");
      await iamUnitOfWork.Users.AddAsync(superUser, cancellationToken);

      Console.WriteLine($" - {supportUser.Email} (System Support)");
      await iamUnitOfWork.Users.AddAsync(supportUser, cancellationToken);

      await iamUnitOfWork.SaveChangesAsync(cancellationToken);
   }

   private async Task SeedScientistsOrgAsync(
      string organizationAdminRoleName,
      string userRoleName,
      IReadOnlyCollection<Role> roles,
      CancellationToken cancellationToken)
   {
      const string organizationCode = "SCIENTISTS";

      var existingOrganization = await organizationRepository.GetByCodeAsync(organizationCode, cancellationToken);

      if (existingOrganization != null)
      {
         seederData.TestOrganizationId = existingOrganization.Id;
         return;
      }

      var organization = Organization.Create(
         OrganizationType.Company,
         organizationCode,
         "Computing Pioneers Society",
         "Foundation of modern Computer Science");

      seederData.TestOrganizationId = organization.Id;

      Console.WriteLine($"Adding organization: {organization.Name}");
      await iamUnitOfWork.Organizations.AddAsync(organization, cancellationToken);

      var passwordHash = Argon2.Hash(DefaultPassword);
      var adminRoleId = roles.First(role => role.Name == organizationAdminRoleName).Id;
      var userRoleId = roles.First(role => role.Name == userRoleName).Id;

      var alanTuring = User.Create("Alan Turing", "alan.turing@enigma.org", passwordHash, DateTime.UtcNow.AddDays(30), organization.Id);
      var adaLovelace = User.Create("Ada Lovelace", "ada.lovelace@analytical.org", passwordHash, DateTime.UtcNow.AddDays(30), organization.Id);
      var graceHopper = User.Create("Grace Hopper", "grace.hopper@cobol.org", passwordHash, DateTime.UtcNow.AddDays(30), organization.Id);
      var johnVonNeumann = User.Create("John von Neumann", "john.vonneumann@architecture.org", passwordHash, DateTime.UtcNow.AddDays(30), organization.Id);
      var claudeShannon = User.Create("Claude Shannon", "claude.shannon@entropy.org", passwordHash, DateTime.UtcNow.AddDays(30), organization.Id);

      var users = new[] { alanTuring, adaLovelace, graceHopper, johnVonNeumann, claudeShannon };


      Console.WriteLine($"Adding admin role to {alanTuring.Email}");
      alanTuring.AddRole(adminRoleId, null);

      foreach (var user in users.Skip(1))
      {
         Console.WriteLine($"Adding user role to {user.Email}");
         user.AddRole(userRoleId, null);
      }

      Console.WriteLine($"Adding users for organization: {organization.Name}");
      foreach (var user in users)
      {
         Console.WriteLine($" - {user.Email}");
         await iamUnitOfWork.Users.AddAsync(user, cancellationToken);
      }

      await iamUnitOfWork.SaveChangesAsync(cancellationToken);
   }
}
