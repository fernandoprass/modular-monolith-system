using IAM.Application.Contracts;
using IAM.Application.Services;
using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Messages;
using IAM.Domain.QueryRepositories;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shared.Application.Contracts;

namespace IAM.Application.Tests.Services;

public class AuthServiceTests
{
   private readonly IUserQueryRepository _userQueryRepositoryMock;
   private readonly IUserService _userServiceMock;
   private readonly IParameterService _parameterServiceMock;
   private readonly IConfiguration _configurationMock;
   private readonly AuthService _authService;

   public AuthServiceTests()
   {
      _userQueryRepositoryMock = Substitute.For<IUserQueryRepository>();
      _userServiceMock = Substitute.For<IUserService>();
      _configurationMock = Substitute.For<IConfiguration>();
      _parameterServiceMock = Substitute.For<IParameterService>();

      _configurationMock["Jwt:Secret"].Returns("dummy-secret-key-with-at-least-32-characters-used-only-for-test");
      _configurationMock["Jwt:ExpirationHours"].Returns("24");
      

      _authService = new AuthService(_userQueryRepositoryMock, _userServiceMock, _parameterServiceMock, _configurationMock);
   }

   [Fact]
   public async Task LoginAsync_HappyPath_ShouldReturnSuccessWithToken()
   {
      var password = "StrongPassword123!";
      var user = CreateValidUser(password, isUserAtive: true, isCustumerActive: true, isLockedUser: false);

      _userQueryRepositoryMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
      var request = new UserLoginRequest(user.Email, password);

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Data?.Token);
      Assert.Equal(user.Email, result.Data.User.Email);
      await _userServiceMock.Received(1).UpdateLastLoginAsync(user.Id, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task LoginAsync_InvalidEmail_ShouldReturnUnauthorized()
   {
      _userQueryRepositoryMock.GetByEmailWithPasswordAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((UserPasswordDto)null!);
      var request = new UserLoginRequest("nonexistent@email.com", "anyPassword");

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.IsType<InvalidEmailPasswordError>(result.Messages.First());
   }

   [Fact]
   public async Task LoginAsync_IncorrectPassword_ShouldReturnUnauthorized()
   {
      var correctPassword = "CorrectPassword123!";
      var wrongPassword = "WrongPassword123!";
      var user = CreateValidUser(correctPassword, isUserAtive: true, isCustumerActive: true, isLockedUser: false);

      _userQueryRepositoryMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
      var request = new UserLoginRequest(user.Email, wrongPassword);

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.IsType<InvalidEmailPasswordError>(result.Messages.First());
   }

   [Fact]
   public async Task LoginAsync_InactiveUser_ShouldReturnUnauthorized()
   {
      var password = "Password123!";
      var user = CreateValidUser(password, isUserAtive: false, isCustumerActive: true, isLockedUser: false);

      _userQueryRepositoryMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
      var request = new UserLoginRequest(user.Email, password);

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.IsType<UnauthorizedAccessError>(result.Messages.First());
   }

    [Fact]
    public async Task LoginAsync_BlockedUser_ShouldReturnAccountLocked()
    {
        var password = "Password123!";
        var user = CreateValidUser(password, isUserAtive: false, isCustumerActive: true, isLockedUser: true);

        _userQueryRepositoryMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        
        var request = new UserLoginRequest(user.Email, password);

        var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.IsType<AccountLockedError>(result.Messages.First());
    }

    [Fact]
   public async Task LoginAsync_InactiveOrganization_ShouldReturnUnauthorized()
   {
      var password = "Password123!";
      var user = CreateValidUser(password, isUserAtive: true, isCustumerActive: false, isLockedUser: false);

      _userQueryRepositoryMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
      var request = new UserLoginRequest(user.Email, password);

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.IsType<UnauthorizedAccessError>(result.Messages.First());
   }

   private UserPasswordDto CreateValidUser(string password, bool isUserAtive, bool isCustumerActive, bool isLockedUser)
   {
      return new UserPasswordDto
      {
         Id = Guid.NewGuid(),
         Name = "Test User",
         Email = "test@example.com",
         PasswordHash = Argon2.Hash(password),
         IsActive = isUserAtive,
         OrganizationIsActive = isCustumerActive,
         OrganizationId = Guid.NewGuid(),
         IsSystemAdmin = false,
         LockedOutUntil = isLockedUser ? DateTime.UtcNow.AddMinutes(50) : null
      };
   }
}