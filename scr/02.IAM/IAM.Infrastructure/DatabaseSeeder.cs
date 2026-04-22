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

namespace IAM.Infrastructure;

public interface IDatabaseSeeder
{
   Task SeedAsync();
}

public class DatabaseSeeder(
   IOrganizationQueryRepository organizationQueryRepository,
   IParameterService parameterService,
   IIamUnitOfWork iamUnitOfWork) : IDatabaseSeeder
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IOrganizationQueryRepository _organizationQueryRepository = organizationQueryRepository;
   private readonly IParameterService _parameterService = parameterService;
   private const string DefaultPassword = "Password123!";

   public async Task SeedAsync()
   {
      await SeedAdminOrganizationAsync();
      await SeedScientistsOrganizationAsync();

      await SeedParamentersAsync();
   }

   private async Task SeedAdminOrganizationAsync()
   {
      var organizationId = Guid.CreateVersion7();
      var organizationCode = "SAASADMIN";

      if (await _organizationQueryRepository.ExistsByCodeAsync(organizationCode)) return;

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

      await _iamUnitOfWork.Organizations.AddAsync(organization);
      await _iamUnitOfWork.Users.AddAsync(superUser);
      await _iamUnitOfWork.Users.AddAsync(User.Create("Internal Support", "support@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), organizationId));
      await _iamUnitOfWork.SaveChangesAsync();
   }

   private async Task SeedScientistsOrganizationAsync()
   {
      var organizationId = Guid.CreateVersion7();
      var organizationCode = "SCIENTISTS";

      if (await _organizationQueryRepository.ExistsByCodeAsync(organizationCode)) return;

      var organization = new Organization
      {
         Id = organizationId,
         Name = "Computing Pioneers Society",
         Code = "SCIENTISTS",
         Type = OrganizationType.Company,
         Description = "Foundation of modern Computer Science",
         CreatedAt = DateTime.UtcNow
      };

      await _iamUnitOfWork.Organizations.AddAsync(organization);

      var passwordHash = Argon2.Hash(DefaultPassword);
      var members = new[]
      {
            ("Alan Turing", "alan.turing@enigma.org"),
            ("Ada Lovelace", "ada.lovelace@analytical.org"),
            ("Grace Hopper", "grace.hopper@cobol.org"),
            ("John von Neumann", "john.vonneumann@architecture.org"),
            ("Claude Shannon", "claude.shannon@entropy.org")
        };

      foreach (var (name, email) in members)
      {
         await _iamUnitOfWork.Users.AddAsync(User.Create(name, email, passwordHash, DateTime.UtcNow.AddDays(30), organizationId));
      }
      await _iamUnitOfWork.SaveChangesAsync();
   }

   private async Task SeedParamentersAsync()
   {
      await AddParameter(IamParam.Security.MaxPasswordAgeInDays, "Maximum Password Age", 
                         "The maximum number of days a password remains valid before the user is required to change it.",
                         ParameterType.String, "dd/MM/yyyy", ParameterOverrideType.UserOwnerId, true);

      await AddParameter(IamParam.Security.LockoutDurationInMins, "Duration of Lockout", 
                         "Duration of lockout in minutes after reaching the maximum number of failed login attempts.",
                         ParameterType.Integer, "60", ParameterOverrideType.None, true);

      await AddParameter(IamParam.Security.MaxFailedLoginAttempts, "Maximum failed logins attemps",
                        "The maximum number of failed login attempts allowed before a user account is locked.",
                         ParameterType.Integer, "3", ParameterOverrideType.None, true);

      await AddParameter(IamParam.Security.JwtExpirationInHours, "JWT Expiration in Hours",
                         "The lifespan of the JSON Web Token (JWT) in hours.",
                          ParameterType.Integer, "24", ParameterOverrideType.None, true);
   }

   private async Task AddParameter(
      string key,
      string title,
      string description,
      ParameterType type,
      string value,
      ParameterOverrideType overrideType,
      bool isVisible)
   {
      await AddParameter(key, title, description, type, value, overrideType, isVisible, null, null, null, null);
   }

   private async Task AddParameter(
      string key,
      string title,
      string description,
      ParameterType type,
      string value,
      ParameterOverrideType overrideType,
      bool isVisible,
      string? validationRegex,
      string? validationErrorCustomMessage,
      string? listItems,
      string? externalListEndpoint)
   {
      if (await _parameterService.ExistsAsync(key)) return;

      var parameterKey = new ParameterKey(key);

      var parameter = new ParameterCreateRequest(
                              parameterKey.Module,
                              parameterKey.Group,
                              parameterKey.Name,
                              title,
                              description,
                              type,
                              value,
                              overrideType,
                              isVisible,
                              validationRegex,
                              validationErrorCustomMessage,
                              listItems,
                              externalListEndpoint);

      await _parameterService.CreateAsync(parameter);
   }
}