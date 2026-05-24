using IAM.Domain;
using Shared.Domain.Enums;
using Shared.Infrastructure;

namespace DatabaseSeeder.Parameters;

public class SeederParametersIam(SharedDbContext dbContext) : SeederParametersBase(dbContext)
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
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
}
