using IAM.Application.Contracts;
using IAM.Application.Services;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Messages;
using IAM.Domain.QueryRepositories;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Enums;
using Shared.Domain.Messages;
using static IAM.Domain.IamPermission;

namespace IAM.Application.Tests.Services;

public class RoleServiceTests
{
   private readonly IIamUnitOfWork _unitOfWorkMock;
   private readonly IUserContext _userContextMock;
   private readonly IRoleValidator _roleValidatorMock;
   private readonly IRoleQueryRepository _roleQueryRepositoryMock;
   private readonly IIamEventPublisher _eventPublisherMock;
   private readonly RoleService _roleService;

   public RoleServiceTests()
   {
      _unitOfWorkMock = Substitute.For<IIamUnitOfWork>();
      _userContextMock = Substitute.For<IUserContext>();
      _roleValidatorMock = Substitute.For<IRoleValidator>();
      _roleQueryRepositoryMock = Substitute.For<IRoleQueryRepository>();
      _eventPublisherMock = Substitute.For<IIamEventPublisher>();

      _roleService = new RoleService(
         _unitOfWorkMock,
         _userContextMock,
         _roleValidatorMock,
         _roleQueryRepositoryMock,
         _eventPublisherMock);
   }

   #region CreateAsync Tests

   [Fact]
   public async Task CreateAsync_WithValidRequest_ShouldCreateRoleSuccessfully()
   {
      var request = CreateRoleCreateRequestRecord(name : "Admin");

      _roleQueryRepositoryMock.NameExistsAsync(request.Name, request.OrganizationId, Arg.Any<CancellationToken>()).Returns(false);

      _roleValidatorMock.ValidateCreate(request, false).Returns(Result.Success());

      var result = await _roleService.CreateAsync(request);
      Assert.True(result.IsSuccess);
      await _unitOfWorkMock.Roles.Received(1).AddAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Roles,
         IamConst.Logger.Action.Create,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         Arg.Any<Guid>(),
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateAsync_WithDuplicateName_ShouldReturnFailure()
   {
      var request = CreateRoleCreateRequestRecord(name: "Admin");

      _roleQueryRepositoryMock.NameExistsAsync(request.Name, request.OrganizationId, Arg.Any<CancellationToken>()).Returns(true);

      var validationError = new RoleDuplicateNameError(request.Name);
      _roleValidatorMock.ValidateCreate(request, true).Returns(Result.Failure(validationError));

      var result = await _roleService.CreateAsync(request);
      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.Roles.DidNotReceive().AddAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateAsync_WithValidationFailure_ShouldReturnFailure()
   {
      var request = CreateRoleCreateRequestRecord(name: string.Empty);

      _roleQueryRepositoryMock.NameExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);

      _roleValidatorMock.ValidateCreate(request, false).Returns(Result.Failure(new NotFoundError()));

      var result = await _roleService.CreateAsync(request);
      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   private static RoleCreateRequest CreateRoleCreateRequestRecord(string name)
   {
      return new RoleCreateRequest(
               Name: name,
               Description: "Administrator role",
               IsDefault: false,
               IsActive: true,
               OrganizationId: Guid.NewGuid());
   }

   #endregion

   #region UpdateAsync Tests

   [Fact]
   public async Task UpdateAsync_WithValidRequest_ShouldUpdateRoleSuccessfully()
   {
      var organizationId = Guid.NewGuid();
      var roleId = Guid.NewGuid();
      var request = CreateRoleUpdateRequestRecord();

      var role = Role.Create("Admin", "Old description", false, true, organizationId);
      var roleWithId = role;
      roleWithId.GetType().GetProperty("Id")?.SetValue(roleWithId, roleId);

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(roleId, Arg.Any<CancellationToken>()).Returns(roleWithId);
      _roleValidatorMock.ValidateUpdate(request, true).Returns(Result.Success());

      var result = await _roleService.UpdateAsync(roleId, request);

      Assert.True(result.IsSuccess);
      _unitOfWorkMock.Roles.Received(1).Update(Arg.Any<Role>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Roles,
         IamConst.Logger.Action.Update,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         roleWithId.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_WithNonExistentRole_ShouldReturnFailure()
   {
      var roleId = Guid.NewGuid();
      var request = CreateRoleUpdateRequestRecord();

      _unitOfWorkMock.Roles.GetByIdAsync(roleId, Arg.Any<CancellationToken>()).Returns((Role?)null);
      _roleValidatorMock.ValidateUpdate(request, false).Returns(Result.Failure(new NotFoundError(IamConst.Entity.Role)));

      var result = await _roleService.UpdateAsync(roleId, request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_WithUnauthorizedUser_ShouldReturnUnauthorized()
   {
      var roleId = Guid.NewGuid();
      var request = CreateRoleUpdateRequestRecord();

      var role = Role.Create("Admin", "Old description", false, true, organizationId : Guid.NewGuid());

      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(roleId, Arg.Any<CancellationToken>()).Returns(role);

      var result = await _roleService.UpdateAsync(roleId, request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   private static RoleUpdateRequest CreateRoleUpdateRequestRecord()
   {
      return new RoleUpdateRequest(
         Name: "Updated Admin",
         Description: "Updated description",
         IsDefault: false,
         IsActive: true);
   }

   #endregion

   #region AssignToUserAsync Tests

   [Fact]
   public async Task AssignToUserAsync_WithValidRequest_ShouldAssignRolesSuccessfully()
   {
      var organizationId = Guid.NewGuid();

      var expiresAt = DateTime.UtcNow.AddMonths(1);
      var request = CreateRoleAssignRequestRecord(expiresAt);

      var user = User.Create("Test User", "test@example.com", "hash", expiresAt, LanguageOptions.English, organizationId);

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(user);
      _roleQueryRepositoryMock.CountRolesByRoleIdsAsync(Arg.Any<IEnumerable<Guid>>(), organizationId, Arg.Any<CancellationToken>()).Returns(1);
      _roleValidatorMock.ValidateAssign(request, true, true).Returns(Result.Success());

      var result = await _roleService.AssignToUserAsync(request);

      Assert.True(result.IsSuccess);
      _unitOfWorkMock.Users.Received(1).Update(Arg.Any<User>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Roles,
         IamConst.Logger.Action.Assign,
         AuditPrivacyLevel.High,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         user.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AssignToUserAsync_WithNonExistentUser_ShouldReturnFailure()
   {
      var request = CreateRoleAssignRequestRecord(expiresAt : null);

      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);
      _roleValidatorMock.ValidateAssign(request, false, false).Returns(Result.Failure(new NotFoundError(IamConst.Entity.User)));

      var result = await _roleService.AssignToUserAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AssignToUserAsync_WithPastExpirationDate_ShouldReturnFailure()
   {
      var organizationId = Guid.NewGuid();

      var request = new RoleAssignRequest(
         UserId: Guid.NewGuid(),
         Roles: [new RoleAssignRoleRequest(RoleId: Guid.NewGuid(), StartsAt: DateTime.UtcNow, ExpiresAt: DateTime.UtcNow.AddDays(-1))]);

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId);

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(user);
      _roleValidatorMock.ValidateAssign(request, true, false).Returns(Result.Failure(new RolesInvalidExpirationError()));
      
      var result = await _roleService.AssignToUserAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AssignToUserAsync_WithUnavailableRoles_ShouldReturnFailure()
   {
      var organizationId = Guid.NewGuid();

      var request = CreateRoleAssignRequestRecord(expiresAt: null);

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId);

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(user);
      _roleQueryRepositoryMock.CountRolesByRoleIdsAsync(Arg.Any<IEnumerable<Guid>>(), organizationId, Arg.Any<CancellationToken>()).Returns(0);
      _roleValidatorMock.ValidateAssign(request, true, false).Returns(Result.Failure(new RolesCannotBeAssignedError()));

      var result = await _roleService.AssignToUserAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AssignToUserAsync_WithUnauthorizedOrganization_ShouldReturnUnauthorized()
   {
      var request = CreateRoleAssignRequestRecord(expiresAt: null);

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId: Guid.NewGuid());

      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _roleService.AssignToUserAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   private static RoleAssignRequest CreateRoleAssignRequestRecord(DateTime? expiresAt)
   {
      return new RoleAssignRequest(
         UserId: Guid.NewGuid(),
         Roles: [new RoleAssignRoleRequest(RoleId: Guid.NewGuid(), StartsAt: DateTime.UtcNow, ExpiresAt: expiresAt)]);
   }

   [Fact]
   public async Task AssignToUserAsync_WithDuplicateRoleIds_ShouldHandleDistinctly()
   {
      var organizationId = Guid.NewGuid();
      var roleId = Guid.NewGuid();
      IEnumerable<Guid>? requestedRoleIds = null;

      // Same role ID listed twice
      var request = new RoleAssignRequest(
         UserId: Guid.NewGuid(),
         Roles:
         [
            new RoleAssignRoleRequest(roleId, StartsAt: DateTime.UtcNow, ExpiresAt: null),
            new RoleAssignRoleRequest(roleId, StartsAt: DateTime.UtcNow, ExpiresAt: null),
         ]);

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId);

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(user);
      _roleQueryRepositoryMock.CountRolesByRoleIdsAsync(
         Arg.Do<IEnumerable<Guid>>(ids => requestedRoleIds = ids.ToArray()),
         organizationId,
         Arg.Any<CancellationToken>()).Returns(1);
      _roleValidatorMock.ValidateAssign(request, true, true).Returns(Result.Success());

      var result = await _roleService.AssignToUserAsync(request);

      Assert.True(result.IsSuccess);
      Assert.Equal([roleId], requestedRoleIds);
      Assert.Single(user.UserRoles);
      Assert.Equal(roleId, user.UserRoles.Single().RoleId);
   }

   #endregion

   #region UnassignFromUserAsync Tests

   [Fact]
   public async Task UnassignFromUserAsync_WithValidRequest_ShouldUnassignRolesSuccessfully()
   {
      var organizationId = Guid.NewGuid();
      var roleId = Guid.NewGuid();

      var request = new RoleUnassignRequest(UserId: Guid.NewGuid(), RoleIds: [roleId]);

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId);
      user.AddRole(roleId, DateTime.UtcNow, null);

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);

      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>())
         .Returns(user);

      _roleValidatorMock.ValidateUnassign(request, true, true)
         .Returns(Result.Success());
      var result = await _roleService.UnassignFromUserAsync(request);
      Assert.True(result.IsSuccess);
      _unitOfWorkMock.Users.Received(1).Update(Arg.Any<User>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Roles,
         IamConst.Logger.Action.Unassign,
         AuditPrivacyLevel.High,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         user.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UnassignFromUserAsync_WithNonExistentUser_ShouldReturnFailure()
   {
      var request = new RoleUnassignRequest(UserId: Guid.NewGuid(), RoleIds: [Guid.NewGuid()]);

      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>())
         .Returns((User?)null);

      _roleValidatorMock.ValidateUnassign(request, false, false)
         .Returns(Result.Failure(new NotFoundError(IamConst.Entity.User)));
      var result = await _roleService.UnassignFromUserAsync(request);
      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UnassignFromUserAsync_WithRoleUserDoesNotHave_ShouldReturnFailure()
   {
      var organizationId = Guid.NewGuid();
      var request = new RoleUnassignRequest(UserId: Guid.NewGuid(), RoleIds: [Guid.NewGuid()]);

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId);
      // User doesn't have this role

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(user);
      _roleValidatorMock.ValidateUnassign(request, true, false).Returns(Result.Failure(new RolesCannotBeUnassignedError()));

      var result = await _roleService.UnassignFromUserAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UnassignFromUserAsync_WithUnauthorizedOrganization_ShouldReturnUnauthorized()
   {
      var roleId = Guid.NewGuid();

      var request = new RoleUnassignRequest(UserId: Guid.NewGuid(), RoleIds: [Guid.NewGuid()]);

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId:Guid.NewGuid());
      user.AddRole(roleId, DateTime.UtcNow, null);

      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdWithRolesAsync(request.UserId, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _roleService.UnassignFromUserAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   #endregion

   #region GetAsync Tests

   [Fact]
   public async Task GetAsync_WithValidName_ShouldReturnRoles()
   {
      var organizationId = Guid.NewGuid();
      var roleName = "Admin";
      var request = new RoleSearchRequest(roleName, null, null, null);
      var roles = new List<RoleDto>
      {
         CreateRoleDto(roleName, organizationId)
      };

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _roleQueryRepositoryMock.GetAsync(
         Arg.Is<RoleSearchRequest>(r => r.Name == roleName && r.OrganizationId == organizationId),
         Arg.Any<CancellationToken>())
         .Returns(roles);

      var result = await _roleService.GetAsync(request);

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Data);
      Assert.Single(result.Data);
   }

   [Fact]
   public async Task GetAsync_WithNoFilters_ShouldReturnAllRoles()
   {
      var organizationId = Guid.NewGuid();
      var request = new RoleSearchRequest(null, null, null, null);
      var roles = new List<RoleDto>
      {
         CreateRoleDto("Admin", organizationId),
         CreateRoleDto("User", organizationId)
      };

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _roleQueryRepositoryMock.GetAsync(
         Arg.Is<RoleSearchRequest>(r => r.Name == null && r.OrganizationId == organizationId),
         Arg.Any<CancellationToken>())
         .Returns(roles);

      var result = await _roleService.GetAsync(request);

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Data);
      Assert.Equal(2, result.Data.Count());
   }

   [Fact]
   public async Task GetAsync_WithSystemAdmin_ShouldKeepRequestedOrganizationFilter()
   {
      var organizationId = Guid.NewGuid();
      var request = new RoleSearchRequest(null, null, null, organizationId);
      var roles = new List<RoleDto>
      {
         CreateRoleDto("Admin", Guid.NewGuid()),
         CreateRoleDto("User", Guid.NewGuid())
      };

      _userContextMock.IsSystemAdmin.Returns(true);
      _roleQueryRepositoryMock.GetAsync(
         Arg.Is<RoleSearchRequest>(r => r.OrganizationId == organizationId),
         Arg.Any<CancellationToken>())
         .Returns(roles);

      var result = await _roleService.GetAsync(request);

      Assert.True(result.IsSuccess);
      await _roleQueryRepositoryMock.Received(1).GetAsync(
         Arg.Is<RoleSearchRequest>(r => r.OrganizationId == organizationId),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetAsync_WithUserId_ShouldReturnUserRoles_WhenUserOwnsOrganization()
   {
      var organizationId = Guid.NewGuid();
      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId);
      var request = new RoleSearchRequest(null, user.Id, true, null);
      var roles = new List<RoleDto>
      {
         CreateRoleDto("Admin", organizationId)
      };

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
      _roleQueryRepositoryMock.GetAsync(
         Arg.Is<RoleSearchRequest>(r => r.UserId == user.Id && r.IsActive == true && r.OrganizationId == organizationId),
         Arg.Any<CancellationToken>())
         .Returns(roles);

      var result = await _roleService.GetAsync(request);

      Assert.True(result.IsSuccess);
      Assert.Single(result.Data!);
   }

   [Fact]
   public async Task GetAsync_WithUserId_ShouldReturnNotFound_WhenUserDoesNotExist()
   {
      var userId = Guid.NewGuid();
      var request = new RoleSearchRequest(null, userId, null, null);

      _unitOfWorkMock.Users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

      var result = await _roleService.GetAsync(request);

      Assert.False(result.IsSuccess);
      await _roleQueryRepositoryMock.DidNotReceive().GetAsync(Arg.Any<RoleSearchRequest>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetAsync_WithUserId_ShouldReturnUnauthorized_WhenUserDoesNotOwnOrganization()
   {
      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, Guid.NewGuid());
      var request = new RoleSearchRequest(null, user.Id, null, null);

      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _roleService.GetAsync(request);

      Assert.False(result.IsSuccess);
      await _roleQueryRepositoryMock.DidNotReceive().GetAsync(Arg.Any<RoleSearchRequest>(), Arg.Any<CancellationToken>());
   }

   #endregion

   #region GetRolePermissionsByUserIdAsync Tests

   [Fact]
   public async Task GetRolePermissionsByUserIdAsync_WithValidUserId_ShouldReturnPermissions()
   {
      var organizationId = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var permissions = new List<Permission>
      {
         Permission.Create("iam", "users", "read", "Read", "Read users", true),
         Permission.Create("iam", "users", "write", "Write", "Write users", true)
      };

      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId);

      _userContextMock.UserOwnerId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
      _roleQueryRepositoryMock.GetRolePermissionsByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(permissions);

      var result = await _roleService.GetRolePermissionsByUserIdAsync(userId);

      Assert.True(result.IsSuccess);
      Assert.Equal(["iam.users.read", "iam.users.write"], result.Data!.Select(p => p.Code));
      await _roleQueryRepositoryMock.Received(1).GetRolePermissionsByUserIdAsync(userId, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetRolePermissionsByUserIdAsync_WithNonExistentUser_ShouldReturnNotFound()
   {
      var userId = Guid.NewGuid();

      _unitOfWorkMock.Users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

      var result = await _roleService.GetRolePermissionsByUserIdAsync(userId);

      Assert.False(result.IsSuccess);
      await _roleQueryRepositoryMock.DidNotReceive().GetRolePermissionsByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetRolePermissionsByUserIdAsync_WithUnauthorizedOrganization_ShouldReturnUnauthorized()
   {
      var user = User.Create("Test User", "test@example.com", "hash", DateTime.UtcNow.AddMonths(1), LanguageOptions.English, organizationId: Guid.NewGuid());

      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

      var result = await _roleService.GetRolePermissionsByUserIdAsync(user.Id);
      Assert.False(result.IsSuccess);
      await _roleQueryRepositoryMock.DidNotReceive().GetRolePermissionsByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
   }

   #endregion

   #region GetPermissionsByRoleIdAsync Tests

   [Fact]
   public async Task GetPermissionsByRoleIdAsync_WithValidRoleId_ShouldReturnPermissions()
   {
      var roleId = Guid.NewGuid();
      var permissions = new List<PermissionDto>
      {
         CreatePermissionDto("read"),
         CreatePermissionDto("write")
      };

      _roleQueryRepositoryMock.GetPermissionsByRoleIdAsync(roleId, Arg.Any<CancellationToken>()).Returns(permissions);

      var result = await _roleService.GetPermissionsByRoleIdAsync(roleId);

      Assert.NotNull(result);
      Assert.Equal(2, result.Count());
      await _roleQueryRepositoryMock.Received(1).GetPermissionsByRoleIdAsync(roleId, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetPermissionsByRoleIdAsync_WithNonExistentRole_ShouldReturnEmptyList()
   {
      var roleId = Guid.NewGuid();

      _roleQueryRepositoryMock.GetPermissionsByRoleIdAsync(roleId, Arg.Any<CancellationToken>()).Returns(new List<PermissionDto>());

      var result = await _roleService.GetPermissionsByRoleIdAsync(roleId);

      Assert.NotNull(result);
      Assert.Empty(result);
   }

   #endregion

   #region SystemAdmin Tests

   [Fact]
   public async Task CreateAsync_WithSystemAdmin_ShouldBypassOrganizationRestriction()
   {
      var request = new RoleCreateRequest(
         Name: "Global Admin",
         Description: "System-wide administrator role",
         IsDefault: false,
         IsActive: true,
         OrganizationId: null);

      _userContextMock.IsSystemAdmin.Returns(true);
      _roleQueryRepositoryMock.NameExistsAsync(request.Name, null, Arg.Any<CancellationToken>()).Returns(false);
      _roleValidatorMock.ValidateCreate(request, false).Returns(Result.Success());

      var result = await _roleService.CreateAsync(request);

      Assert.True(result.IsSuccess);
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_WithSystemAdmin_ShouldBypassOrganizationCheck()
   {
      var roleId = Guid.NewGuid();
      var request = new RoleUpdateRequest(
         Name: "Updated Admin",
         Description: "Updated description",
         IsDefault: false,
         IsActive: true);

      var role = Role.Create("Admin", "Old description", false, true, organizationId: Guid.NewGuid());

      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(true);
      _unitOfWorkMock.Roles.GetByIdAsync(roleId, Arg.Any<CancellationToken>()).Returns(role);
      _roleValidatorMock.ValidateUpdate(request, true).Returns(Result.Success());

      var result = await _roleService.UpdateAsync(roleId, request);

      Assert.True(result.IsSuccess);
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   #endregion

   #region Cancellation Token Tests

   [Fact]
   public async Task CreateAsync_ShouldPassCancellationTokenToRepository()
   {
      var cts = new CancellationTokenSource();
      var receivedToken = CancellationToken.None;

      var request = new RoleCreateRequest(
         Name: "Admin",
         Description: "Administrator role",
         IsDefault: false,
         IsActive: true,
         OrganizationId: Guid.NewGuid());

      _roleQueryRepositoryMock.NameExistsAsync(
            Arg.Any<string>(),
            Arg.Any<Guid?>(),
            Arg.Do<CancellationToken>(ct => receivedToken = ct))
         .Returns(false);
      _roleValidatorMock.ValidateCreate(request, false).Returns(Result.Failure(new NotFoundError()));

      await _roleService.CreateAsync(request, cts.Token);

      Assert.Equal(cts.Token, receivedToken);
   }

   #endregion

   private static RoleDto CreateRoleDto(string name, Guid? organizationId)
   {
      return new RoleDto(Guid.NewGuid(), name, "Role description", true, false, organizationId);
   }

   private static PermissionDto CreatePermissionDto(string name)
   {
      return new PermissionDto(
         Guid.NewGuid(),
         "iam",
         "permissions",
         name,
         $"iam.permissions.{name}",
         name,
         $"{name} permission",
         true);
   }
}



