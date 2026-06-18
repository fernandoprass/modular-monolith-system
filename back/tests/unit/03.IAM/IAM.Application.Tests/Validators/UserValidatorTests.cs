using FluentAssertions;
using IAM.Application.Validators;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Messages;
using Isopoh.Cryptography.Argon2;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Messages;

namespace IAM.Application.Tests.Validators;

public class UserValidatorTests
{
   private readonly UserValidator _validator;

   public UserValidatorTests()
   {
      _validator = new UserValidator();
   }

   #region CreateUser
   [Fact]
   public void ValidateCreate_ShouldBeSuccess_WhenAllDataIsValid()
   {
      var request = new UserCreateRequest("Dev Senior", "test@example.com", "Strong#Pass123", LanguageOptions.English, Guid.NewGuid());

      var result = _validator.ValidateCreate(request, emailAlreadyExists: false, organizationExists: true);

      Assert.True(result.IsSuccess);
   }

   [Fact]
   public void ValidateCreate_ShouldHaveError_WhenEmailAlreadyExists()
   {
      var request = new UserCreateRequest("Dev Senior", "exists@example.com", "Strong#Pass123", LanguageOptions.English, Guid.NewGuid());

      var result = _validator.ValidateCreate(request, emailAlreadyExists: true, organizationExists: true);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is EmailAlreadyExistError);
   }

   [Fact]
   public void ValidateCreate_ShouldHaveError_WhenOrganizationDoesNotExist()
   {
      var request = new UserCreateRequest("Dev Senior", "test@example.com", "Strong#Pass123", LanguageOptions.English, Guid.NewGuid());

      var result = _validator.ValidateCreate(request, emailAlreadyExists: false, organizationExists: false);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is NotFoundError && m.Show().Contains(IamConst.Entity.Organization));
   }

   [Theory]
   [InlineData("Valid User", "test@domain.com", "Pass123!", false, true)]      // Case 1: Everything is valid and email is unique 
   [InlineData("Valid User", "duplicate@domain.com", "Pass123!", true, false)] // Case 2: Data is valid but email ALREADY exists in the database
   [InlineData("Ab", "test@domain.com", "Pass123!", false, false)]             // Case 3: Email is unique but Title fails template validation (too short)
   [InlineData("Valid User", "test@domain.com", "Password!", false, false)]    // Case 4: Email is unique but Password fails template validation (no digit)
   public void ValidateCreateForNewOrganization_ShouldHandleValidationFlow(
        string name,
        string email,
        string password,
        bool emailAlreadyExists,
        bool expectedSuccess)
   {
      var request = new OrganizationUserCreateRequest(name, email, password);

      var result = _validator.ValidateCreateForNewOrganization(request, emailAlreadyExists);

      result.IsSuccess.Should().Be(expectedSuccess);

      if (!expectedSuccess && emailAlreadyExists)
      {
         // Verify if the specific "Already Exists" error is returned
         result.Messages.Should().Contain(m => m is EmailAlreadyExistError);
      }
   }

   [Fact]
   public void ValidateCreateForNewOrganization_ShouldIncludeEmailInDuplicateError()
   {
      var email = "existing@domain.com";
      var request = new OrganizationUserCreateRequest("Valid Name", email, "Pass123!");

      var result = _validator.ValidateCreateForNewOrganization(request, emailAlreadyExists: true);

      result.IsSuccess.Should().BeFalse();
      var error = result.Messages.OfType<EmailAlreadyExistError>().FirstOrDefault();
      error.Should().NotBeNull();
      // Ensuring the error message contains the specific email passed in the request
      result.Messages.First().Show().Should().Contain(email);
   }
   #endregion

   #region Updates

   [Fact]
   public void ValidateUpdate_ShouldHaveError_WhenIdIsNull()
   {
      var request = new UserUpdateRequest("New Name", true, LanguageOptions.English);

      var result = _validator.ValidateUpdate(null, request);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is NotFoundError);
   }

   [Fact]
   public void ValidateUpdatePassword_ShouldBeSuccess_WhenCredentialsAreValid()
   {
      var oldPassword = "Old#Password123";
      var user = User.Create("User Test", "test@email.com", Argon2.Hash(oldPassword), DateTime.UtcNow.AddDays(30), LanguageOptions.English, Guid.NewGuid());
      var request = new UserUpdatePasswordRequest(oldPassword, "New#StrongPass88");

      var result = _validator.ValidateUpdatePassword(user, request);

      Assert.True(result.IsSuccess);
      Assert.Empty(result.Messages);
   }

   [Fact]
   public void ValidateUpdatePassword_ShouldHaveError_WhenOldPasswordIsIncorrect()
   {
      var user = User.Create("User Test", "test@email.com", Argon2.Hash("Correct#123"), DateTime.UtcNow.AddDays(30), LanguageOptions.English, Guid.NewGuid());
      var request = new UserUpdatePasswordRequest("Wrong#123", "New#StrongPass88");

      var result = _validator.ValidateUpdatePassword(user, request);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is PasswordNotValidError);
   }

   [Fact]
   public void ValidateUpdateOrganizationAdmin_ShouldBeSuccess_WhenUserIsSystemAdmin()
   {
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, Guid.NewGuid());
      var request = new UserUpdateOrganizationAdminRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: true, isSupportUser: false, isOrganizationAdmin: false, organizationId: Guid.NewGuid());

      var result = _validator.ValidateUpdateOrganizationAdmin(user, userContext, request);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUpdateOrganizationAdmin_ShouldBeSuccess_WhenUserIsOrganizationAdminForSameOrganization()
   {
      var organizationId = Guid.NewGuid();
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, organizationId);
      var request = new UserUpdateOrganizationAdminRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: false, isOrganizationAdmin: true, isSupportUser: false, organizationId: organizationId);

      var result = _validator.ValidateUpdateOrganizationAdmin(user, userContext, request);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUpdateOrganizationAdmin_ShouldHaveError_WhenTargetUserNotFound()
   {
      var request = new UserUpdateOrganizationAdminRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: true, isOrganizationAdmin: false, isSupportUser: false, organizationId: Guid.NewGuid());

      var result = _validator.ValidateUpdateOrganizationAdmin(null, userContext, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is NotFoundError);
   }

   [Fact]
   public void ValidateUpdateOrganizationAdmin_ShouldHaveError_WhenCurrentUserIsNotAdmin()
   {
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, Guid.NewGuid());
      var request = new UserUpdateOrganizationAdminRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: false, isOrganizationAdmin: false, isSupportUser: false, organizationId: user.OrganizationId);

      var result = _validator.ValidateUpdateOrganizationAdmin(user, userContext, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is Domain.Messages.UnauthorizedAccessError);
   }

   [Fact]
   public void ValidateUpdateOrganizationAdmin_ShouldHaveError_WhenOrganizationAdminUpdatesAnotherOrganization()
   {
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, Guid.NewGuid());
      var request = new UserUpdateOrganizationAdminRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: false, isOrganizationAdmin: true, isSupportUser: false, organizationId: Guid.NewGuid());

      var result = _validator.ValidateUpdateOrganizationAdmin(user, userContext, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is Domain.Messages.UnauthorizedAccessError);
   }

   [Fact]
   public void ValidateUpdateSupportUser_ShouldBeSuccess_WhenUserIsSystemAdmin()
   {
     Guid organizationId = Guid.NewGuid();
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, organizationId);
      var request = new UserUpdateSupportUserRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: true, isSupportUser: false, isOrganizationAdmin: false, organizationId: organizationId);

      var result = _validator.ValidateUpdateSupportUser(user, userContext, request);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUpdateSupportUser_ShouldBeSuccess_WhenUserIsSupportUserForSameOrganization()
   {
      var organizationId = Guid.NewGuid();
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, organizationId);
      var request = new UserUpdateSupportUserRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: false, isSupportUser: true, isOrganizationAdmin: false, organizationId: organizationId);

      var result = _validator.ValidateUpdateSupportUser(user, userContext, request);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUpdateSupportUser_ShouldHaveError_WhenTargetUserNotFound()
   {
      var request = new UserUpdateSupportUserRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: true, isSupportUser: false, isOrganizationAdmin: false, organizationId: Guid.NewGuid());

      var result = _validator.ValidateUpdateSupportUser(null, userContext, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is NotFoundError);
   }

   [Fact]
   public void ValidateUpdateSupportUser_ShouldHaveError_WhenCurrentUserIsNotSupportOrSystemAdmin()
   {
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, Guid.NewGuid());
      var request = new UserUpdateSupportUserRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: false, isSupportUser: false, isOrganizationAdmin: true, organizationId: user.OrganizationId);

      var result = _validator.ValidateUpdateSupportUser(user, userContext, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is Domain.Messages.UnauthorizedAccessError);
   }

   [Fact]
   public void ValidateUpdateSupportUser_ShouldHaveError_WhenSupportUserUpdatesAnotherOrganization()
   {
      var user = User.Create("User Test", "test@email.com", "hash", DateTime.UtcNow.AddDays(30), LanguageOptions.English, Guid.NewGuid());
      var request = new UserUpdateSupportUserRequest(true);
      var userContext = CreateUserContext(isSystemAdmin: false, isSupportUser: true, isOrganizationAdmin: false, organizationId: Guid.NewGuid());

      var result = _validator.ValidateUpdateSupportUser(user, userContext, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is Domain.Messages.UnauthorizedAccessError);
   }
   #endregion

   private static IUserContext CreateUserContext(bool isSystemAdmin, bool isSupportUser, bool isOrganizationAdmin, Guid organizationId)
   {
      var userContext = Substitute.For<IUserContext>();
      userContext.IsSystemAdmin.Returns(isSystemAdmin);
      userContext.IsSupportUser.Returns(isSupportUser);
      userContext.IsOrganizationAdmin.Returns(isOrganizationAdmin);
      userContext.OrganizationId.Returns(organizationId);

      return userContext;
   }
}

