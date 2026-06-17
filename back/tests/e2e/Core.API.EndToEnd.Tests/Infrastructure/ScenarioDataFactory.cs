using IAM.Domain.DTOs.Requests;
using IAM.Domain.Enums;

namespace Core.API.EndToEnd.Tests.Infrastructure;

internal static class ScenarioDataFactory
{
   public const string Password = "Password123!";
   public const string UpdatedPassword = "Password456!";

   public static OrganizationCreateRequest CreateOrganization()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new OrganizationCreateRequest(
         OrganizationType.Company,
         $"E2E Test - Acme {suffix}",
         $"acme{suffix}",
         $"E2E Test - Organization {suffix}",
         "en",
         new OrganizationUserCreateRequest(
            $"E2E Test - Admin {suffix}",
            $"admin-{suffix}@example.com",
            Password));
   }

   public static OrganizationUpdateRequest UpdateOrganization()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new OrganizationUpdateRequest(
         $"E2E Test - Updated Acme {suffix}",
         $"E2E Test - Updated organization {suffix}",
         true,
         "en");
   }

   public static OrganizationUpdateCodeRequest UpdateOrganizationCode()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new OrganizationUpdateCodeRequest($"updated{suffix}");
   }

   public static UserCreateRequest CreateUser(Guid organizationId)
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new UserCreateRequest(
         $"E2E Test - User {suffix}",
         $"user-{suffix}@example.com",
         Password,
         "en",
         organizationId);
   }

   public static UserUpdateRequest UpdateUser()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new UserUpdateRequest(
         $"E2E Test - Updated User {suffix}",
         true,
         "pt-BR");
   }

   public static UserUpdatePasswordRequest UpdatePassword()
   {
      return new UserUpdatePasswordRequest(Password, UpdatedPassword);
   }

   public static RoleCreateRequest CreateRole(Guid organizationId)
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new RoleCreateRequest(
         $"E2E Test - Role {suffix}",
         $"E2E Test - Role description {suffix}",
         false,
         true,
         organizationId);
   }

   public static RoleUpdateRequest UpdateRoleAsDefault()
   {
      var suffix = Guid.NewGuid().ToString("N")[..8];

      return new RoleUpdateRequest(
         $"E2E Test - Updated Role {suffix}",
         $"E2E Test - Updated role description {suffix}",
         true,
         true);
   }
}
