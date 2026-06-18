using IAM.Application.Contracts;
using IAM.Application.Services;
using IAM.Domain;
using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Messages;
using IAM.Domain.QueryRepositories;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using SharedPermissionService = Shared.Application.Contracts.IRolePermissionCache;

namespace IAM.Application.Tests.Services;

public class AuthServiceTests
{
   private readonly IRoleQueryRepository _roleQueryRepositoryMock;
   private readonly IUserService _userServiceMock;
   private readonly IParameterService _parameterServiceMock;
   private readonly SharedPermissionService _permissionServiceMock;
   private readonly IIamEventPublisher _eventPublisherMock;
   private readonly IConfiguration _configurationMock;
   private readonly AuthService _authService;

   public AuthServiceTests()
   {
      _roleQueryRepositoryMock = Substitute.For<IRoleQueryRepository>();
      _userServiceMock = Substitute.For<IUserService>();
      _configurationMock = Substitute.For<IConfiguration>();
      _parameterServiceMock = Substitute.For<IParameterService>();
      _permissionServiceMock = Substitute.For<SharedPermissionService>();
      _eventPublisherMock = Substitute.For<IIamEventPublisher>();

      _configurationMock["Jwt:Secret"].Returns("dummy-secret-key-with-at-least-32-characters-used-only-for-test");
      _configurationMock["Jwt:ExpirationHours"].Returns("24");

      _authService = new AuthService(
         _roleQueryRepositoryMock,
         _userServiceMock,
         _parameterServiceMock,
         _permissionServiceMock,
         _eventPublisherMock,
         _configurationMock);
   }

   [Fact]
   public async Task LoginAsync_HappyPath_ShouldReturnSuccessWithToken()
   {
      var password = "StrongPassword123!";
      var user = CreateValidUser(password, isUserAtive: true, isCustumerActive: true, isLockedUser: false);

      _userServiceMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
      var request = new UserLoginRequest(user.Email, password);

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Data?.Token);
      Assert.Equal(user.Email, result.Data.User.Email);
      await _userServiceMock.Received(1).UpdateLastLoginAsync(user.Id, Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Authentication,
         IamConst.Logger.Action.LoginSuccess,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(),
         Arg.Is<string>(description => description.Contains(user.Email)),
         user.Id,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task LoginAsync_HappyPath_ShouldHydratePermissionCache()
   {
      var password = "StrongPassword123!";
      var roleId = Guid.NewGuid();
      var user = CreateValidUser(password, isUserAtive: true, isCustumerActive: true, isLockedUser: false);
      user.RoleIds = [roleId];
      var permissionList = new List<RolePermissionCodeDto> { new(roleId, IamPermission.Users.Read) };

      _userServiceMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
      _roleQueryRepositoryMock.GetPermissionCodesByRoleIdsAsync(user.RoleIds, Arg.Any<CancellationToken>()).Returns(permissionList);
      var request = new UserLoginRequest(user.Email, password);

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      await _permissionServiceMock.Received(1).SetPermissionsAsync(
         roleId.ToString(),
         Arg.Is<IEnumerable<string>>(permissions => permissions.Contains(IamPermission.Users.Read)),
         Arg.Any<DateTime>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task LoginAsync_InvalidEmail_ShouldReturnUnauthorized()
   {
      _userServiceMock.GetByEmailWithPasswordAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((UserPasswordDto)null!);
      var request = new UserLoginRequest("nonexistent@email.com", "anyPassword");

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.IsType<InvalidEmailPasswordError>(result.Messages.First());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Authentication,
         IamConst.Logger.Action.LoginFail,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(),
         Arg.Is<string>(description => description.Contains(request.Email)),
         null,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task LoginAsync_IncorrectPassword_ShouldReturnUnauthorized()
   {
      var correctPassword = "CorrectPassword123!";
      var wrongPassword = "WrongPassword123!";
      var user = CreateValidUser(correctPassword, isUserAtive: true, isCustumerActive: true, isLockedUser: false);

      _userServiceMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
      var request = new UserLoginRequest(user.Email, wrongPassword);

      var result = await _authService.LoginAsync(request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.IsType<InvalidEmailPasswordError>(result.Messages.First());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Authentication,
         IamConst.Logger.Action.LoginFail,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(),
         Arg.Is<string>(description => description.Contains(user.Email)),
         user.Id,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task LoginAsync_InactiveUser_ShouldReturnUnauthorized()
   {
      var password = "Password123!";
      var user = CreateValidUser(password, isUserAtive: false, isCustumerActive: true, isLockedUser: false);

      _userServiceMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
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

        _userServiceMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        
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

      _userServiceMock.GetByEmailWithPasswordAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
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
