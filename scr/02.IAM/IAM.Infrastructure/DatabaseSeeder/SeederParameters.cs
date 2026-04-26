using IAM.Domain;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Enums;

namespace IAM.Infrastructure.DatabaseSeeder;

public class SeederParameters(IParameterService parameterService)
{
   public async Task SeedAsync()
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
      if (await parameterService.ExistsAsync(key)) return;

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

      await parameterService.CreateAsync(parameter);
   }
}
