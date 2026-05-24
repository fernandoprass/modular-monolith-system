using FluentAssertions;
using IAM.Application.Contracts;
using IAM.Application.Services;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Messages;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Enums;

namespace IAM.Application.Tests.Services;

public class UserServiceTests
{
   private readonly IIamUnitOfWork _unitOfWorkMock;
   private readonly IParameterService _parameterServiceMock;
   private readonly IUserValidator _userValidatorMock;
   private readonly IUserContext _userContextMock;
   private readonly IUserRepository _userRepositoryMock;
   private readonly IUserQueryRepository _userQueryRepositoryMock;
   private readonly IIamAuditLogger _auditLoggerMock;
   private readonly IIamEmailNotifier _emailNotifierMock;
   private readonly UserService _userService;

   public UserServiceTests()
   {
      _unitOfWorkMock = Substitute.For<IIamUnitOfWork>();
      _parameterServiceMock = Substitute.For<IParameterService>();
      _userContextMock = Substitute.For<IUserContext>();
      _userValidatorMock = Substitute.For<IUserValidator>();
      _userRepositoryMock = Substitute.For<IUserRepository>();
      _userQueryRepositoryMock = Substitute.For<IUserQueryRepository>();
      _auditLoggerMock = Substitute.For<IIamAuditLogger>();
      _emailNotifierMock = Substitute.For<IIamEmailNotifier>();

      _unitOfWorkMock.Users.Returns(_userRepositoryMock);
      _userContextMock.UserOwnerId.Returns(Guid.CreateVersion7());

      _userService = new UserService(
          _unitOfWorkMock,
          _parameterServiceMock,
          _userContextMock,
          _userValidatorMock,
          _userRepositoryMock,
          _userQueryRepositoryMock,
          _auditLoggerMock,
          _emailNotifierMock);
   }

   [Fact]
   public async Task CreateUserAsync_ShouldReturnForbiddenOrganizationError_WhenOperatorIdDoesNotMatch()
   {
      var request = new UserCreateRequest(string.Empty, "test@test.com", string.Empty, Guid.NewGuid());

      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)30);

      var result = await _userService.CreateUserAsync(request, true, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);

      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateUserAsync_ShouldReturnValidationErrors_WhenValidatorFails()
   {
      var request = new UserCreateRequest("John Smith", "test@test.com", string.Empty, _userContextMock.UserOwnerId);

      _userQueryRepositoryMock.GetIdByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)30);

      _userValidatorMock.ValidateCreate(request, organizationExists: true, emailAlreadyExists: true)
          .Returns(Result.Failure(new EmailAlreadyExistError(request.Email)));

      var result = await _userService.CreateUserAsync(request, true, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is EmailAlreadyExistError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateUserAsync_ShouldSaveUser_WhenRequestIsValid()
   {
      var request = new UserCreateRequest("John Doe", "new@test.com", "SecurePassword123", _userContextMock.UserOwnerId);

      _userQueryRepositoryMock.GetIdByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(Guid.Empty);

      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)30);

      _userValidatorMock.ValidateCreate(request, organizationExists: true, emailAlreadyExists: false).Returns(Result.Success());

      var result = await _userService.CreateUserAsync(request, true, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();

      await _unitOfWorkMock.Users.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _emailNotifierMock.Received(1).NotifyAsync(
         IamConst.EmailTemplate.UserWelcome,
         request.OrganizationId,
         Arg.Any<Guid>(),
         request.Email.ToLower().Trim(),
         IamConst.Logger.Feature.Users,
         Arg.Any<IReadOnlyDictionary<string, string>>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_ShouldReturnForbiddenOrganizationError_EvenWhenUserDoesNotExist()
   {
      var userId = Guid.NewGuid();
      _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User)null);

      var result = await _userService.DeleteAsync(userId, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);

      await _unitOfWorkMock.Users.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdatePasswordAsync_ShouldUpdateHashAndExpiration_WhenRequestIsValid()
   {
      var request = new UserUpdatePasswordRequest("OldPass123", "NewSecurePass123");
      var user = User.Create("Name", "test@test.com", "OldHash", DateTime.UtcNow, _userContextMock.UserOwnerId);

      _userContextMock.UserId.Returns(user.Id);
      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)90); // 90 days
      _userValidatorMock.ValidateUpdatePassword(user, user.Id, request).Returns(Result.Success());

      var result = await _userService.UpdatePasswordAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      user.PasswordExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(90), TimeSpan.FromSeconds(5));
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _auditLoggerMock.Received(1).LogAsync(
         IamConst.Logger.Feature.Users,
         IamConst.Logger.Action.UpdatePassword,
         AuditPrivacyLevel.High,
         Arg.Any<string>(),
         user.Id,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
   }


   [Fact]
   public async Task UpdatePasswordAsync_ShouldReturnError_WhenValidatorFails()
   {
      var request = new UserUpdatePasswordRequest("OldPass123", "NewSecurePass123");
      var user = User.Create("Name", "test@test.com", "OldHash", DateTime.UtcNow, _userContextMock.UserOwnerId);

      _userContextMock.UserId.Returns(Guid.NewGuid());
      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)90); // 90 days
      _userValidatorMock.ValidateUpdatePassword(user, _userContextMock.UserId, request).Returns(Result.Failure(new Shared.Domain.Messages.UnauthorizedAccessError()));

      var result = await _userService.UpdatePasswordAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_ShouldUpdateUserFields_WhenRequestIsValid()
   {
      var request = new UserUpdateRequest("Updated Name", false);
      var user = User.Create("Original Name", "test@test.com", "hash", DateTime.UtcNow, _userContextMock.UserOwnerId);

      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _userValidatorMock.ValidateUpdate(user.Id, request).Returns(Result.Success());

      var result = await _userService.UpdateAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      user.Name.Should().Be("Updated Name");
      user.IsActive.Should().BeFalse();
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _auditLoggerMock.Received(1).LogAsync(
         IamConst.Logger.Feature.Users,
         IamConst.Logger.Action.Update,
         AuditPrivacyLevel.Medium,
         Arg.Any<string>(),
         user.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_ShouldReturnForbidden_WhenUserBelongsToAnotherOrganization()
   {
      var differentOrganizationId = Guid.NewGuid();
      var request = new UserUpdateRequest("Name", true);
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, differentOrganizationId);

      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _userService.UpdateAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateLastLoginAsync_ShouldUpdateTimestampAndSave()
   {
      var userId = Guid.NewGuid();
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, _userContextMock.UserOwnerId);
      var initialLastLogin = user.LastLoginAt;

      _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _userService.UpdateLastLoginAsync(userId, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      user.LastLoginAt.Should().BeAfter(initialLastLogin ?? DateTime.MinValue);
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task ValidateUserForNewOrganizationAsync_ShouldReturnError_WhenEmailAlreadyExists()
   {
      var request = new OrganizationUserCreateRequest("John Admas", "exists@test.com", "Str0ngP4ssw0d!");
      _userQueryRepositoryMock.GetIdByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(Guid.NewGuid()); // Email exists

      _userValidatorMock.ValidateCreateForNewOrganization(request, true)
          .Returns(Result.Failure(new EmailAlreadyExistError(request.Email)));

      var result = await _userService.ValidateUserForNewOrganizationAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is EmailAlreadyExistError);
   }
}
