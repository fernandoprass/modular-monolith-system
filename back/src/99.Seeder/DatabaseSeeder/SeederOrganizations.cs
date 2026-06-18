using DatabaseSeeder.Interfaces;
using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using Isopoh.Cryptography.Argon2;
using Shared.Domain;

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

      await SeedAdminOrgAsync(cancellationToken);
      await SeedScientistsOrgAsync(cancellationToken);

      Console.WriteLine("Finished adding organizations...");
      Console.WriteLine();
   }

   private async Task SeedAdminOrgAsync(CancellationToken cancellationToken)
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
         "Internal system management and support", 
         LanguageOptions.English);

      seederData.SaaSOrganizationId= organization.Id;

      var passwordHash = Argon2.Hash(DefaultPassword);

      var superUser = User.Create("System Root", "admin@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), LanguageOptions.English, organization.Id);
      superUser.IsSystemAdmin = true;

      superUser.AddRole(seederData.SysAdminRoleId, DateTime.UtcNow, null);
      organization.CreatedBy = superUser.Id;

      var supportUser = User.Create("Internal Support", "support@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), LanguageOptions.English, organization.Id);
      superUser.IsSupportUser = true;
      supportUser.AddRole(seederData.SysAdminRoleId, DateTime.UtcNow, null);

      Console.WriteLine($"Adding organization: {organization.Name}");
      await iamUnitOfWork.Organizations.AddAsync(organization, cancellationToken);

      Console.WriteLine($"Adding users for organization: {organization.Name}");
      Console.WriteLine($" - {superUser.Email} (System Admin)");
      await iamUnitOfWork.Users.AddAsync(superUser, cancellationToken);

      Console.WriteLine($" - {supportUser.Email} (System Support)");
      await iamUnitOfWork.Users.AddAsync(supportUser, cancellationToken);

      await iamUnitOfWork.SaveChangesAsync(cancellationToken);
   }

   private async Task SeedScientistsOrgAsync(CancellationToken cancellationToken)
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
         "Foundation of modern Computer Science",
         LanguageOptions.English);

      seederData.TestOrganizationId = organization.Id;

      Console.WriteLine($"Adding organization: {organization.Name}");
      await iamUnitOfWork.Organizations.AddAsync(organization, cancellationToken);

      var passwordHash = Argon2.Hash(DefaultPassword);

      var alanTuring = User.Create("Alan Turing", "alan.turing@enigma.org", passwordHash, DateTime.UtcNow.AddDays(30), LanguageOptions.English, organization.Id);
      var adaLovelace = User.Create("Ada Lovelace", "ada.lovelace@analytical.org", passwordHash, DateTime.UtcNow.AddDays(30), LanguageOptions.English, organization.Id);
      var graceHopper = User.Create("Grace Hopper", "grace.hopper@cobol.org", passwordHash, DateTime.UtcNow.AddDays(30), LanguageOptions.English, organization.Id);
      var johnVonNeumann = User.Create("John von Neumann", "john.vonneumann@architecture.org", passwordHash, DateTime.UtcNow.AddDays(30), LanguageOptions.English, organization.Id);
      var claudeShannon = User.Create("Claude Shannon", "claude.shannon@entropy.org", passwordHash, DateTime.UtcNow.AddDays(30), LanguageOptions.English, organization.Id);

      var users = new[] { alanTuring, adaLovelace, graceHopper, johnVonNeumann, claudeShannon };


      Console.WriteLine($"Adding admin role to {alanTuring.Email}");
      alanTuring.AddRole(seederData.OrganizationAdminRoleId, DateTime.UtcNow, null);
      alanTuring.IsOrganizationAdmin = true;

      foreach (var user in users.Skip(1))
      {
         Console.WriteLine($"Adding user role to {user.Email}");
         user.AddRole(seederData.UserRoleId, DateTime.UtcNow, null);
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
