using FluentAssertions;
using IAM.Application.Validators;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Messages;
using Isopoh.Cryptography.Argon2;
using Shared.Domain.Messages;


namespace IAM.Application.Tests.Validators;

public class UserValidatorTests
{
   private readonly UserValidator _validator;

   public UserValidatorTests()
   {
      _validator = new UserValidator();
   }

   [Fact]
   public void ValidateCreate_ShouldBeSuccess_WhenAllDataIsValid()
   {
      var request = new UserCreateRequest("Dev Senior", "test@example.com", "Strong#Pass123", Guid.NewGuid());

      var result = _validator.ValidateCreate(request, emailAlreadyExists: false, customerExists: true);

      Assert.True(result.IsSuccess);
   }

   [Fact]
   public void ValidateCreate_ShouldHaveError_WhenEmailAlreadyExists()
   {
      var request = new UserCreateRequest("Dev Senior", "exists@example.com", "Strong#Pass123", Guid.NewGuid());

      var result = _validator.ValidateCreate(request, emailAlreadyExists: true, customerExists: true);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is EmailAlreadyExistError);
   }

   [Fact]
   public void ValidateCreate_ShouldHaveError_WhenCustomerDoesNotExist()
   {
      var request = new UserCreateRequest("Dev Senior", "test@example.com", "Strong#Pass123", Guid.NewGuid());

      var result = _validator.ValidateCreate(request, emailAlreadyExists: false, customerExists: false);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is NotFoundError && m.Show().Contains(IamConst.Entity.Customer));
   }

   [Fact]
   public void ValidateUpdate_ShouldHaveError_WhenIdIsNull()
   {
      var request = new UserUpdateRequest("New Name", true);

      var result = _validator.ValidateUpdate(null, request);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is NotFoundError);
   }

   [Fact]
   public void ValidateUpdatePassword_ShouldBeSuccess_WhenCredentialsAreValid()
   {
      var oldPassword = "Old#Password123";
      var user = User.Create("User Test", "test@email.com", Argon2.Hash(oldPassword), DateTime.UtcNow.AddDays(30), Guid.NewGuid());
      var request = new UserUpdatePasswordRequest(oldPassword, "New#StrongPass88");

      var result = _validator.ValidateUpdatePassword(user, user.Id, request);

      Assert.True(result.IsSuccess);
      Assert.Empty(result.Messages);
   }

   [Fact]
   public void ValidateUpdatePassword_ShouldHaveError_WhenOldPasswordIsIncorrect()
   {
      var user = User.Create("User Test", "test@email.com", Argon2.Hash("Correct#123"), DateTime.UtcNow.AddDays(30), Guid.NewGuid());
      var request = new UserUpdatePasswordRequest("Wrong#123", "New#StrongPass88");

      var result = _validator.ValidateUpdatePassword(user, user.Id, request);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is PasswordNotValidError);
   }

   [Fact]
   public void ValidateUpdatePassword_ShouldHaveError_WhenUserIsDifferentOfLoggedUser()
   {
      var oldPassword = "Old#Password123";
      var user = User.Create("User Test", "test@email.com", Argon2.Hash(oldPassword), DateTime.UtcNow.AddDays(30), Guid.NewGuid());
      var request = new UserUpdatePasswordRequest(oldPassword, "New#StrongPass88");

      var result = _validator.ValidateUpdatePassword(user, Guid.NewGuid(), request);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is UnauthorizedAccessError);
   }

   [Fact]
   public void ValidateUpdatePassword_ShouldHaveError_WhenUserNotFound()
   {
      var request = new UserUpdatePasswordRequest("any", "New#StrongPass88");

      var result = _validator.ValidateUpdatePassword(null, Guid.NewGuid(), request);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, m => m is NotFoundError);
   }

   [Theory]
   [InlineData("Valid User", "test@domain.com", "Pass123!", false, true)]      // Case 1: Everything is valid and email is unique 
   [InlineData("Valid User", "duplicate@domain.com", "Pass123!", true, false)] // Case 2: Data is valid but email ALREADY exists in the database
   [InlineData("Ab", "test@domain.com", "Pass123!", false, false)]             // Case 3: Email is unique but Title fails template validation (too short)
   [InlineData("Valid User", "test@domain.com", "Password!", false, false)]    // Case 4: Email is unique but Password fails template validation (no digit)
   public void ValidateCreateForNewCustomer_ShouldHandleValidationFlow(
        string name,
        string email,
        string password,
        bool emailAlreadyExists,
        bool expectedSuccess)
   {
      var request = new CustomerUserCreateRequest(name, email, password);

      var result = _validator.ValidateCreateForNewCustomer(request, emailAlreadyExists);

      result.IsSuccess.Should().Be(expectedSuccess);

      if (!expectedSuccess && emailAlreadyExists)
      {
         // Verify if the specific "Already Exists" error is returned
         result.Messages.Should().Contain(m => m is EmailAlreadyExistError);
      }
   }

   [Fact]
   public void ValidateCreateForNewCustomer_ShouldIncludeEmailInDuplicateError()
   {
      var email = "existing@domain.com";
      var request = new CustomerUserCreateRequest("Valid Name", email, "Pass123!");

      var result = _validator.ValidateCreateForNewCustomer(request, emailAlreadyExists: true);

      result.IsSuccess.Should().BeFalse();
      var error = result.Messages.OfType<EmailAlreadyExistError>().FirstOrDefault();
      error.Should().NotBeNull();
      // Ensuring the error message contains the specific email passed in the request
      result.Messages.First().Show().Should().Contain(email);
   }
}

