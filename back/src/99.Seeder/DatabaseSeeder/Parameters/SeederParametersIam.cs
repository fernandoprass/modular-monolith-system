using DatabaseSeeder.Interfaces;
using IAM.Domain;
using Shared.Domain.Enums;
using Shared.Infrastructure;

namespace DatabaseSeeder.Parameters;

public class SeederParametersIam(ISeederData seederData,SharedDbContext dbContext) : SeederParametersBase(dbContext)
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      await AddParameterAsync(
         IamParam.Security.MaxPasswordAgeInDays,
         "Maximum Password Age",
         "The maximum number of days a password remains valid before the user is required to change it.",
         ParameterType.Integer,
         value: "90",
         ParameterOverrideType.OrganizationId,
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

      await AddParameterAsync(
         IamParam.Role.DefaultRoleIdForNewOrganization,
         "Default Roles for Organization",
         "The default role Id assigned to the organization's administrator user when it is created.",
         ParameterType.UUID,
         value: seederData.OrganizationAdminRoleId.ToString(),
         ParameterOverrideType.None,
         isVisible: false,
         cancellationToken);

      await AddParameterAsync(
         IamParam.Role.DefaultRoleIdForNewUser,
         "Default Roles for User",
         "The default role Id assigned to the user when it is created.",
         ParameterType.UUID,
         value: seederData.UserRoleId.ToString(),
         ParameterOverrideType.None,
         isVisible: false,
         cancellationToken);
   }
}
