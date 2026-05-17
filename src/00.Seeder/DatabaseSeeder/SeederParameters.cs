using IAM.Domain;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Infrastructure;

namespace DatabaseSeeder;

public class SeederParameters(SharedDbContext dbContext)
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to add parameters...");
      await SeedIamParameters(cancellationToken);

      await dbContext.SaveChangesAsync(cancellationToken);

      Console.WriteLine("Finished adding parameters...");
      Console.WriteLine();
   }

   private async Task SeedIamParameters(CancellationToken cancellationToken)
   {
      await AddParameterAsync(
         IamParam.Security.MaxPasswordAgeInDays,
         "Maximum Password Age",
         "The maximum number of days a password remains valid before the user is required to change it.",
         ParameterType.Integer,
         value: "90",
         ParameterOverrideType.UserOwnerId,
         isVisible: true,
         cancellationToken);

      await AddParameterAsync(
         IamParam.Security.LockoutDurationInMins,
         "Duration of Lockout",
         "Duration of lockout in minutes after reaching the maximum number of failed login attempts.",
         ParameterType.Integer,
         value: "60",
         ParameterOverrideType.None,
         isVisible: true,
         cancellationToken);

      await AddParameterAsync(
         IamParam.Security.MaxFailedLoginAttempts,
         "Maximum failed logins attemps",
         "The maximum number of failed login attempts allowed before a user account is locked.",
         ParameterType.Integer,
         value: "3",
         ParameterOverrideType.None,
         isVisible: true,
         cancellationToken);

      await AddParameterAsync(
         IamParam.Security.JwtExpirationInHours,
         "JWT Expiration in Hours",
         "The lifespan of the JSON Web Token (JWT) in hours.",
         ParameterType.Integer,
         value: "24",
         ParameterOverrideType.None,
         isVisible: true,
         cancellationToken);
   }

   private async Task AddParameterAsync(
      string key,
      string title,
      string description,
      ParameterType type,
      string value,
      ParameterOverrideType overrideType,
      bool isVisible,
      CancellationToken cancellationToken)
   {
      var exists = await dbContext.Parameters.AnyAsync(parameter => parameter.Key == key, cancellationToken);

      if (exists) return;

      Console.WriteLine($"Parameter: {key}");

      var parameterKey = new ParameterKey(key);
      var parameter = Parameter.Create(
         parameterKey.Module,
         parameterKey.Group,
         parameterKey.Name,
         title,
         description,
         type,
         value,
         validationRegex: null,
         validationErrorCustomMessage: null,
         listItems: null,
         externalListEndpoint: null,
         overrideType,
         isVisible);

      await dbContext.Parameters.AddAsync(parameter, cancellationToken);
   }
}
