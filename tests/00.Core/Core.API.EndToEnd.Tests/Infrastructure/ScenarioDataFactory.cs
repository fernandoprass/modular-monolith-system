using IAM.Domain.DTOs.Requests;
using IAM.Domain.Enums;

namespace Core.API.EndToEnd.Tests.Infrastructure;

internal static class ScenarioDataFactory
{
   public const string Password = "Password123!";

   public static OrganizationCreateRequest CreateOrganization()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new OrganizationCreateRequest(
         OrganizationType.Company,
         $"Acme {suffix}",
         $"acme{suffix}",
         $"Organization {suffix}",
         "en",
         new OrganizationUserCreateRequest(
            $"Admin {suffix}",
            $"admin-{suffix}@example.com",
            Password));
   }

   public static OrganizationUpdateRequest UpdateOrganization()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new OrganizationUpdateRequest(
         $"Updated Acme {suffix}",
         $"Updated organization {suffix}",
         true,
         "en");
   }

   public static OrganizationUpdateCodeRequest UpdateOrganizationCode()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new OrganizationUpdateCodeRequest($"updated{suffix}");
   }
}
