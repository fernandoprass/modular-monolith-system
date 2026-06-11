using FluentAssertions;
using IAM.Domain.Entities;
using Shared.Domain;

namespace IAM.Domain.Tests.Entities;

public class UserTests
{
   [Fact]
   public void Create_ShouldNormalizeEmailAndSetOrganizationAdminFalse()
   {
      var organizationId = Guid.NewGuid();
      var passwordExpiresAt = DateTime.UtcNow.AddDays(30);

      var user = User.Create("Ana", " ANA@Example.COM ", "hash", passwordExpiresAt, LanguageOptions.English, organizationId);

      user.Email.Should().Be("ana@example.com");
      user.IsOrganizationAdmin.Should().BeFalse();
      user.IsSystemAdmin.Should().BeFalse();
      user.IsActive.Should().BeTrue();
      user.OrganizationId.Should().Be(organizationId);
      user.PasswordExpiresAt.Should().Be(passwordExpiresAt);
   }

   [Fact]
   public void CreateOrganizationAdmin_ShouldSetOrganizationAdminTrue()
   {
      var user = User.CreateOrganizationAdmin(
         "Ana",
         "ana@example.com",
         "hash",
         DateTime.UtcNow.AddDays(30),
         isOrganizationAdmin: true,
         LanguageOptions.English,
         Guid.NewGuid());

      user.IsOrganizationAdmin.Should().BeTrue();
   }

   [Fact]
   public void RegisterLastSuccessfullyLogin_ShouldResetFailedAttemptsAndLockout()
   {
      var user = CreateUser();
      user.RegisterFailedLoginAttempt(maxFailedAttempts: 1, lockoutMinutes: 10);

      user.RegisterLastSuccessfullyLogin();

      user.NumFailedLoginAttempts.Should().Be(0);
      user.LockedOutUntil.Should().BeNull();
      user.LastLoginAt.Should().NotBeNull();
   }

   [Fact]
   public void RegisterFailedLoginAttempt_ShouldIncreaseAttempts()
   {
      var user = CreateUser();

      user.RegisterFailedLoginAttempt(maxFailedAttempts: 3, lockoutMinutes: 10);

      user.NumFailedLoginAttempts.Should().Be(1);
      user.LockedOutUntil.Should().BeNull();
   }

   [Fact]
   public void RegisterFailedLoginAttempt_ShouldLockUser_WhenMaxAttemptsReached()
   {
      var user = CreateUser();

      user.RegisterFailedLoginAttempt(maxFailedAttempts: 1, lockoutMinutes: 10);

      user.NumFailedLoginAttempts.Should().Be(1);
      user.LockedOutUntil.Should().NotBeNull();
      user.LockedOutUntil.Should().BeAfter(DateTime.UtcNow);
   }

   [Fact]
   public void AddRole_ShouldAddRole_WhenRoleDoesNotExist()
   {
      var user = CreateUser();
      var roleId = Guid.NewGuid();
      var expiresAt = DateTime.UtcNow.AddDays(10);

      user.AddRole(roleId, DateTime.UtcNow, expiresAt);

      user.UserRoles.Should().ContainSingle(role =>
         role.RoleId == roleId &&
         role.UserId == user.Id &&
         role.ExpiresAt == expiresAt);
   }

   [Fact]
   public void AddRole_ShouldNotAddDuplicateRole()
   {
      var user = CreateUser();
      var roleId = Guid.NewGuid();

      user.AddRole(roleId, DateTime.UtcNow, null);
      user.AddRole(roleId, DateTime.UtcNow, null);

      user.UserRoles.Should().ContainSingle(role => role.RoleId == roleId);
   }

   [Fact]
   public void RemoveRole_ShouldRemoveRole_WhenRoleExists()
   {
      var user = CreateUser();
      var roleId = Guid.NewGuid();
      user.AddRole(roleId, DateTime.UtcNow, null);

      user.RemoveRole(roleId);

      user.UserRoles.Should().BeEmpty();
   }

   [Fact]
   public void RemoveRole_ShouldDoNothing_WhenRoleDoesNotExist()
   {
      var user = CreateUser();
      var roleId = Guid.NewGuid();
      user.AddRole(roleId, DateTime.UtcNow, null);

      user.RemoveRole(Guid.NewGuid());

      user.UserRoles.Should().ContainSingle(role => role.RoleId == roleId);
   }

   [Fact]
   public void ClearRoles_ShouldRemoveAllRoles()
   {
      var user = CreateUser();
      user.AddRole(Guid.NewGuid(), DateTime.UtcNow, null);
      user.AddRole(Guid.NewGuid(), DateTime.UtcNow, null);

      user.ClearRoles();

      user.UserRoles.Should().BeEmpty();
   }

   private static User CreateUser()
   {
      return User.Create(
         "Ana",
         "ana@example.com",
         "hash",
         DateTime.UtcNow.AddDays(30),
         LanguageOptions.English,
         Guid.NewGuid());
   }
}
