using IAM.Domain;
using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
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

public class DatabaseSeeder : IDatabaseSeeder
{
   private readonly IIamUnitOfWork _iamUnitOfWork;
   private readonly IParameterService _parameterService;
   private const string DefaultPassword = "Password123!";

   public DatabaseSeeder(
      IParameterService parameterService,
      IIamUnitOfWork iamUnitOfWork)
   {
      _parameterService = parameterService;
      _iamUnitOfWork = iamUnitOfWork;
   }

   public async Task SeedAsync()
   {
      //await SeedAdminCustomerAsync();
      //await SeedScientistsCustomerAsync();

      await SeedParamentersAsync();
   }

   private async Task SeedAdminCustomerAsync()
   {
      var customerId = Guid.CreateVersion7();
      if (await _iamUnitOfWork.Customers.ExistsAsync(customerId)) return;

      var customer = new Customer
      {
         Id = customerId,
         Name = "SaaS Internal Administration",
         Code = "SAASADMIN",
         Type = CustomerType.Company,
         Description = "Internal system management and support",
         IsMaster = true, // Following our Master Customer rule
         CreatedAt = DateTime.UtcNow
      };

      var passwordHash = Argon2.Hash(DefaultPassword);

      var superUser = User.Create("System Root", "admin@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), customerId);
      superUser.IsSystemAdmin = true;
      customer.CreatedBy = superUser.Id;

      await _iamUnitOfWork.Customers.AddAsync(customer);
      await _iamUnitOfWork.Users.AddAsync(superUser);
      await _iamUnitOfWork.Users.AddAsync(User.Create("Internal Support", "support@saas.com", passwordHash, DateTime.UtcNow.AddDays(30), customerId));
      await _iamUnitOfWork.SaveChangesAsync();
   }

   private async Task SeedScientistsCustomerAsync()
   {
      var customerId = Guid.CreateVersion7();
      if (await _iamUnitOfWork.Customers.ExistsAsync(customerId)) return;

      var customer = new Customer
      {
         Id = customerId,
         Name = "Computing Pioneers Society",
         Code = "SCIENTISTS",
         Type = CustomerType.Company,
         Description = "Foundation of modern Computer Science",
         CreatedAt = DateTime.UtcNow
      };

      await _iamUnitOfWork.Customers.AddAsync(customer);

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
         await _iamUnitOfWork.Users.AddAsync(User.Create(name, email, passwordHash, DateTime.UtcNow.AddDays(30), customerId));
      }
      await _iamUnitOfWork.SaveChangesAsync();
   }

   private async Task SeedParamentersAsync()
   {
      await AddParameter(SharedParam.System.DateFormat, "Date Format", "The Date Format, accepted characters: 'dd' for the day, 'MM' for the month, 'yyyy' for the year, and '-' or '/' as a separator.",
                         ParameterType.Integer, "60", ParameterOverrideType.None, false);

      await AddParameter(IamParam.Security.PasswordExpireTime, "Password expiration time", "Password expiration time, in days.",
                         ParameterType.String, "dd/MM/yyyy", ParameterOverrideType.UserOwnerId, true);
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
      var paramExists = await _parameterService.ExistsAsync(key);

      if (!paramExists)
      {
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
}