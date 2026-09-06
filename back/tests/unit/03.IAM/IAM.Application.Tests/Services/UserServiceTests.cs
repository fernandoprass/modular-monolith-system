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
using Shared.Domain;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace IAM.Application.Tests.Services;

public class UserServiceTests
{
   private readonly IIamUnitOfWork _unitOfWorkMock;
   private readonly IParameterService _parameterServiceMock;
   private readonly IRoleService _roleServiceMock;
   private readonly IUserValidator _userValidatorMock;
   private readonly IUserContext _userContextMock;
   private readonly IUserRepository _userRepositoryMock;
   private readonly IUserQueryRepository _userQueryRepositoryMock;
   private readonly IIamEventPublisher _eventPublisherMock;
   private readonly UserService _userService;

   public UserServiceTests()
   {
      _unitOfWorkMock = Substitute.For<IIamUnitOfWork>();
      _parameterServiceMock = Substitute.For<IParameterService>();
      _roleServiceMock = Substitute.For<IRoleService>();
      _userContextMock = Substitute.For<IUserContext>();
      _userValidatorMock = Substitute.For<IUserValidator>();
      _userRepositoryMock = Substitute.For<IUserRepository>();
      _userQueryRepositoryMock = Substitute.For<IUserQueryRepository>();
      _eventPublisherMock = Substitute.For<IIamEventPublisher>();

      _unitOfWorkMock.Users.Returns(_userRepositoryMock);
      _userContextMock.OrganizationId.Returns(Guid.CreateVersion7());

      _userService = new UserService(
          _unitOfWorkMock,
          _parameterServiceMock,
          _roleServiceMock,
          _userContextMock,
          _userValidatorMock,
          _userQueryRepositoryMock,
          _eventPublisherMock);
   }

   [Fact]
   public async Task CreateUserAsync_ShouldReturnForbiddenOrganizationError_WhenOperatorIdDoesNotMatch()
   {
      var request = new UserCreateRequest(string.Empty, "test@test.com", string.Empty, LanguageOptions.English, Guid.NewGuid());

      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)30);
      _parameterServiceMock.GetGuidAsync(IamParam.Role.DefaultRoleIdForNewUser, Arg.Any<CancellationToken>()).Returns(Guid.CreateVersion7());
      _roleServiceMock.GetDefaultRolesByOrganizationIdAsync(request.OrganizationId, Arg.Any<CancellationToken>()).Returns(new List<Guid>());

      var result = await _userService.CreateUserAsync(request, true, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);

      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateUserAsync_ShouldReturnValidationErrors_WhenValidatorFails()
   {
      var request = new UserCreateRequest("John Smith", "test@test.com", string.Empty, LanguageOptions.English, _userContextMock.OrganizationId);

      _userQueryRepositoryMock.GetIdByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)30);
      _parameterServiceMock.GetGuidAsync(IamParam.Role.DefaultRoleIdForNewUser, Arg.Any<CancellationToken>()).Returns(Guid.CreateVersion7());
      _roleServiceMock.GetDefaultRolesByOrganizationIdAsync(request.OrganizationId, Arg.Any<CancellationToken>()).Returns(new List<Guid>());

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
      var request = new UserCreateRequest("John Doe", "new@test.com", "SecurePassword123", LanguageOptions.English, _userContextMock.OrganizationId);

      _userQueryRepositoryMock.GetIdByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(Guid.Empty);

      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)30);
      _parameterServiceMock.GetGuidAsync(IamParam.Role.DefaultRoleIdForNewUser, Arg.Any<CancellationToken>()).Returns(Guid.CreateVersion7());
      _roleServiceMock.GetDefaultRolesByOrganizationIdAsync(request.OrganizationId, Arg.Any<CancellationToken>()).Returns(new List<Guid> { Guid.CreateVersion7() });

      _userValidatorMock.ValidateCreate(request, organizationExists: true, emailAlreadyExists: false).Returns(Result.Success());

      var result = await _userService.CreateUserAsync(request, true, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      AssertCrudSuccess(result.Messages, SharedTranslatedMessagesProvider.CrudCreatedSuccessInfo, "User created successfully.", "Usuário criado(a) com sucesso.");

      await _unitOfWorkMock.Users.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyUserAsync(
         IamConst.Templates.UserWelcome,
         request.OrganizationId,
         Arg.Any<Guid>(),
         request.Email.ToLower().Trim(),
         IamConst.Logger.Feature.Users,
         Arg.Any<IReadOnlyDictionary<string, string>>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_ShouldReturnUnauthorized_WhenUserDoesNotExistAndCurrentUserIsNotSystemAdmin()
   {
      var userId = Guid.NewGuid();
      _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User)null);

      var result = await _userService.DeleteAsync(userId, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);

      await _unitOfWorkMock.Users.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_ShouldReturnNotFound_WhenUserDoesNotExistAndCurrentUserIsSystemAdmin()
   {
      var userId = Guid.NewGuid();

      _userContextMock.IsSystemAdmin.Returns(true);
      _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

      var result = await _userService.DeleteAsync(userId, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.NotFoundError);

      await _unitOfWorkMock.Users.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_ShouldReturnDeletedSuccessMessage_WhenRequestIsValid()
   {
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, _userContextMock.OrganizationId);

      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _userService.DeleteAsync(user.Id, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      AssertCrudSuccess(result.Messages, SharedTranslatedMessagesProvider.CrudDeletedSuccessInfo, "User deleted successfully.", "Usuário removido(a) com sucesso.");
      await _unitOfWorkMock.Users.Received(1).DeleteAsync(user.Id, Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdatePasswordAsync_ShouldUpdateHashAndExpiration_WhenRequestIsValid()
   {
      var passwordExpirasAt = DateTime.UtcNow;
      var request = new UserUpdatePasswordRequest("OldPass123", "NewSecurePass123");
      var user = User.Create("Name", "test@test.com", "OldHash", passwordExpirasAt, LanguageOptions.English, _userContextMock.OrganizationId);

      _userContextMock.UserId.Returns(user.Id);
      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)90); // 90 days
      _userValidatorMock.ValidateUpdatePassword(user, request).Returns(Result.Success());

      var result = await _userService.UpdatePasswordAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      user.PasswordExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(90), TimeSpan.FromSeconds(10));
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Users,
         IamConst.Logger.Action.UpdatePassword,
         AuditPrivacyLevel.High,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         user.Id,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
   }


   [Fact]
   public async Task UpdatePasswordAsync_ShouldReturnError_WhenValidatorFails()
   {
      var request = new UserUpdatePasswordRequest("OldPass123", "NewSecurePass123");
      var user = User.Create("Name", "test@test.com", "OldHash", DateTime.UtcNow, LanguageOptions.English, _userContextMock.OrganizationId);

      _userContextMock.UserId.Returns(Guid.NewGuid());
      user.Id = _userContextMock.UserId;

      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _parameterServiceMock.GetShortIntAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((short)90); // 90 days
      _userValidatorMock.ValidateUpdatePassword(user, request).Returns(Result.Failure(new Shared.Domain.Messages.UnauthorizedAccessError()));

      var result = await _userService.UpdatePasswordAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_ShouldUpdateUserFields_WhenRequestIsValid()
   {
      var request = new UserUpdateRequest("Updated Name", false, LanguageOptions.PortugueseBrazil);
      var user = User.Create("Original Name", "test@test.com", "hash", DateTime.UtcNow, LanguageOptions.Spanish, _userContextMock.OrganizationId);

      _userRepositoryMock.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _userValidatorMock.ValidateUpdate(user.Id, request).Returns(Result.Success());

      var result = await _userService.UpdateAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      AssertCrudSuccess(result.Messages, SharedTranslatedMessagesProvider.CrudUpdatedSuccessInfo, "User updated successfully.", "Usuário atualizado(a) com sucesso.");
      user.Name.Should().Be("Updated Name");
      user.IsActive.Should().BeFalse();
      user.Language.Should().Be(LanguageOptions.PortugueseBrazil);
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Users,
         IamConst.Logger.Action.Update,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(), 
         Arg.Any<string>(),
         user.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_ShouldReturnForbidden_WhenUserBelongsToAnotherOrganization()
   {
      var differentOrganizationId = Guid.NewGuid();
      var request = new UserUpdateRequest("Name", true, LanguageOptions.English);
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, differentOrganizationId);

      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _userService.UpdateAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateOrganizationAdminAsync_ShouldUpdateAndAudit_WhenUserIsSystemAdmin()
   {
      var request = new UserUpdateOrganizationAdminRequest(true);
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, Guid.NewGuid());

      _userContextMock.IsSystemAdmin.Returns(true);
      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _userValidatorMock.ValidateUpdateOrganizationAdmin(
         user,
         _userContextMock,
         request)
         .Returns(Result.Success());

      var result = await _userService.UpdateOrganizationAdminAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      user.IsOrganizationAdmin.Should().BeTrue();
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Users,
         IamConst.Logger.Action.UpdateOrganizationAdmin,
         AuditPrivacyLevel.High,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         user.Id,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateOrganizationAdminAsync_ShouldUpdateAndAudit_WhenUserIsOrganizationAdminForSameOrganization()
   {
      var organizationId = _userContextMock.OrganizationId;
      var request = new UserUpdateOrganizationAdminRequest(true);
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, organizationId);

      _userContextMock.IsSystemAdmin.Returns(false);
      _userContextMock.IsOrganizationAdmin.Returns(true);
      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _userValidatorMock.ValidateUpdateOrganizationAdmin(
         user,
         _userContextMock,
         request)
         .Returns(Result.Success());

      var result = await _userService.UpdateOrganizationAdminAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      user.IsOrganizationAdmin.Should().BeTrue();
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateOrganizationAdminAsync_ShouldReturnForbidden_WhenCurrentUserIsNotAdmin()
   {
      var request = new UserUpdateOrganizationAdminRequest(true);

      _userContextMock.IsSystemAdmin.Returns(false);
      _userContextMock.IsOrganizationAdmin.Returns(false);
      _userValidatorMock.ValidateUpdateOrganizationAdmin(
         null,
         _userContextMock,
         request)
         .Returns(Result.Failure(new Shared.Domain.Messages.UnauthorizedAccessError()));

      var result = await _userService.UpdateOrganizationAdminAsync(Guid.NewGuid(), request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateOrganizationAdminAsync_ShouldReturnForbidden_WhenOrganizationAdminUpdatesAnotherOrganization()
   {
      var request = new UserUpdateOrganizationAdminRequest(true);
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, Guid.NewGuid());

      _userContextMock.IsSystemAdmin.Returns(false);
      _userContextMock.IsOrganizationAdmin.Returns(true);
      _userRepositoryMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _userValidatorMock.ValidateUpdateOrganizationAdmin(
         user,
         _userContextMock,
         request)
         .Returns(Result.Failure(new Shared.Domain.Messages.UnauthorizedAccessError()));

      var result = await _userService.UpdateOrganizationAdminAsync(user.Id, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.UnauthorizedAccessError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateOrganizationAdminAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
   {
      var request = new UserUpdateOrganizationAdminRequest(true);
      var userId = Guid.NewGuid();

      _userContextMock.IsSystemAdmin.Returns(true);
      _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);
      _userValidatorMock.ValidateUpdateOrganizationAdmin(
         null,
         _userContextMock,
         request)
         .Returns(Result.Failure(new Shared.Domain.Messages.NotFoundError(IamConst.Entity.User)));

      var result = await _userService.UpdateOrganizationAdminAsync(userId, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is Shared.Domain.Messages.NotFoundError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateLastLoginAsync_ShouldUpdateTimestampAndSave()
   {
      var userId = Guid.NewGuid();
      var user = User.Create("Name", "test@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, _userContextMock.OrganizationId);
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

   private static void AssertCrudSuccess(
      IEnumerable<Myce.Response.Messages.Message> messages,
      string expectedCode,
      string expectedEnglish,
      string expectedPortuguese)
   {
      var message = messages.Should().ContainSingle(m => m.Code == expectedCode).Subject;

      message.Show(LanguageOptions.English).Should().Be(expectedEnglish);
      message.Show(LanguageOptions.PortugueseBrazil).Should().Be(expectedPortuguese);
   }
}
