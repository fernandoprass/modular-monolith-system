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
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace IAM.Application.Tests.Services;

public class PermissionServiceTests
{
   private readonly IIamUnitOfWork _unitOfWorkMock;
   private readonly IUserContext _userContextMock;
   private readonly IPermissionValidator _permissionValidatorMock;
   private readonly IPermissionQueryRepository _permissionQueryRepositoryMock;
   private readonly IRolePermissionCacheInvalidator _rolePermissionAuthorizationCacheMock;
   private readonly IIamEventPublisher _eventPublisherMock;
   private readonly PermissionService _permissionService;

   public PermissionServiceTests()
   {
      _unitOfWorkMock = Substitute.For<IIamUnitOfWork>();
      _userContextMock = Substitute.For<IUserContext>();
      _permissionValidatorMock = Substitute.For<IPermissionValidator>();
      _permissionQueryRepositoryMock = Substitute.For<IPermissionQueryRepository>();
      _rolePermissionAuthorizationCacheMock = Substitute.For<IRolePermissionCacheInvalidator>();
      _eventPublisherMock = Substitute.For<IIamEventPublisher>();

      _permissionService = new PermissionService(
         _unitOfWorkMock,
         _userContextMock,
         _permissionValidatorMock,
         _permissionQueryRepositoryMock,
         _rolePermissionAuthorizationCacheMock,
         _eventPublisherMock);
   }

   #region CreateAsync Tests

   [Fact]
   public async Task CreateAsync_WithValidRequest_ShouldCreatePermissionSuccessfully()
   {
      var request = CreatePermissionCreateRequestRecord();

      _permissionQueryRepositoryMock.CodeExistsAsync("iam.users.create", Arg.Any<CancellationToken>()).Returns(false);
      _permissionValidatorMock.ValidateCreate(request, false).Returns(Result.Success());

      var result = await _permissionService.CreateAsync(request);

      Assert.True(result.IsSuccess);
      await _unitOfWorkMock.Permissions.Received(1).AddAsync(Arg.Any<Permission>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateAsync_WithDuplicateCode_ShouldReturnFailure()
   {
      var request = CreatePermissionCreateRequestRecord();

      _permissionQueryRepositoryMock.CodeExistsAsync("iam.users.create", Arg.Any<CancellationToken>()).Returns(true);
      _permissionValidatorMock.ValidateCreate(request, true).Returns(Result.Failure(new PermissionDuplicateError("iam.users.create")));

      var result = await _permissionService.CreateAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.Permissions.DidNotReceive().AddAsync(Arg.Any<Permission>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateAsync_WithValidationFailure_ShouldReturnFailure()
   {
      var request = CreatePermissionCreateRequestRecord(module: string.Empty);

      _permissionQueryRepositoryMock.CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
      _permissionValidatorMock.ValidateCreate(request, false).Returns(Result.Failure(new PermissionDuplicateError("")));

      var result = await _permissionService.CreateAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   #endregion

   #region UpdateAsync Tests

   [Fact]
   public async Task UpdateAsync_WithValidRequest_ShouldUpdatePermissionSuccessfully()
   {
      var permissionId = Guid.NewGuid();
      var request = CreatePermissionUpdateRequestRecord();
      var permission = CreatePermissionWithId(permissionId);

      _unitOfWorkMock.Permissions.GetByIdAsync(permissionId, Arg.Any<CancellationToken>()).Returns(permission);
      _permissionQueryRepositoryMock.CodeExistsAsync("iam.users.update", permissionId, Arg.Any<CancellationToken>()).Returns(false);
      _permissionValidatorMock.ValidateUpdate(request, false, true).Returns(Result.Success());

      var result = await _permissionService.UpdateAsync(permissionId, request);

      Assert.True(result.IsSuccess);
      _unitOfWorkMock.Permissions.Received(1).Update(permission);
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_WithNonExistentPermission_ShouldReturnFailure()
   {
      var permissionId = Guid.NewGuid();
      var request = CreatePermissionUpdateRequestRecord();

      _unitOfWorkMock.Permissions.GetByIdAsync(permissionId, Arg.Any<CancellationToken>()).Returns((Permission?)null);
      _permissionQueryRepositoryMock.CodeExistsAsync("iam.users.update", permissionId, Arg.Any<CancellationToken>()).Returns(false);
      _permissionValidatorMock.ValidateUpdate(request, false, false).Returns(Result.Failure(new NotFoundError(IamConst.Entity.Permission)));

      var result = await _permissionService.UpdateAsync(permissionId, request);

      Assert.False(result.IsSuccess);
      _unitOfWorkMock.Permissions.DidNotReceive().Update(Arg.Any<Permission>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_WithDuplicateCode_ShouldReturnFailure()
   {
      var permissionId = Guid.NewGuid();
      var request = CreatePermissionUpdateRequestRecord();
      var permission = CreatePermissionWithId(permissionId);

      _unitOfWorkMock.Permissions.GetByIdAsync(permissionId, Arg.Any<CancellationToken>()).Returns(permission);
      _permissionQueryRepositoryMock.CodeExistsAsync("iam.users.update", permissionId, Arg.Any<CancellationToken>()).Returns(true);
      _permissionValidatorMock.ValidateUpdate(request, true, true).Returns(Result.Failure(new PermissionDuplicateError("iam.users.update")));

      var result = await _permissionService.UpdateAsync(permissionId, request);

      Assert.False(result.IsSuccess);
      _unitOfWorkMock.Permissions.DidNotReceive().Update(Arg.Any<Permission>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   #endregion

   #region DeleteAsync Tests

   [Fact]
   public async Task DeleteAsync_WithExistingPermission_ShouldDeletePermissionSuccessfully()
   {
      var permissionId = Guid.NewGuid();
      var permission = CreatePermissionWithId(permissionId);

      _unitOfWorkMock.Permissions.GetByIdAsync(permissionId, Arg.Any<CancellationToken>()).Returns(permission);

      var result = await _permissionService.DeleteAsync(permissionId);

      Assert.True(result.IsSuccess);
      await _unitOfWorkMock.Permissions.Received(1).DeleteAsync(permissionId, Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_WithNonExistentPermission_ShouldReturnFailure()
   {
      var permissionId = Guid.NewGuid();

      _unitOfWorkMock.Permissions.GetByIdAsync(permissionId, Arg.Any<CancellationToken>()).Returns((Permission?)null);

      var result = await _permissionService.DeleteAsync(permissionId);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.Permissions.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   #endregion

   #region GetByParams Tests

   [Fact]
   public async Task GetByParams_WithValidRequest_ShouldReturnPermissions()
   {
      var request = new PermissionSearchRequest(roleId: null, Module: "iam", Resource: null, Action: null, Title: null, IncludeInactive: false);
      var permissions = new List<PermissionDto>
      {
         CreatePermissionDto("create"),
         CreatePermissionDto("update")
      };

      var pagedResult = new PagedResultDto<PermissionDto>(
         Items: permissions,
         PageNumber: 1,
         PageSize: 10,
         TotalCount: 2,
         TotalPages: 1
      );

      _permissionQueryRepositoryMock.GetByParams(request, Arg.Any<CancellationToken>()).Returns(pagedResult);

      var result = await _permissionService.GetByParams(request);

      Assert.Equal(1, result.TotalPages);
      Assert.Equal(2, result.TotalCount);
   }

   #endregion

   #region AssignToRoleAsync Tests

   [Fact]
   public async Task AssignToRoleAsync_WithValidRequest_ShouldAssignPermissionsSuccessfully()
   {
      var organizationId = Guid.NewGuid();
      var existingPermissionId = Guid.NewGuid();
      var newPermissionId = Guid.NewGuid();
      var request = new RolePermissionAssignRequest(RoleId: Guid.NewGuid(), PermissionIds: [existingPermissionId, newPermissionId]);
      var role = Role.Create("Admin", "Admin role", false, true, organizationId);
      role.AddPermission(existingPermissionId);

      _userContextMock.OrganizationId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(request.RoleId, Arg.Any<CancellationToken>()).Returns(role);
      _unitOfWorkMock.Permissions.CountByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(2);
      _permissionValidatorMock.ValidateAssign(request, true, true).Returns(Result.Success());

      var result = await _permissionService.AssignToRoleAsync(request);

      Assert.True(result.IsSuccess);
      Assert.Equal(2, role.RolePermissions.Count);
      Assert.Contains(role.RolePermissions, rp => rp.PermissionId == existingPermissionId);
      Assert.Contains(role.RolePermissions, rp => rp.PermissionId == newPermissionId);
      _unitOfWorkMock.Roles.Received(1).Update(role);
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _rolePermissionAuthorizationCacheMock.Received(1).RemoveAsync(role.Id, Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Permissions,
         IamConst.Logger.Action.Assign,
         AuditPrivacyLevel.High,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         role.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AssignToRoleAsync_WithUnavailablePermissions_ShouldReturnFailure()
   {
      var organizationId = Guid.NewGuid();
      var request = new RolePermissionAssignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);
      var role = Role.Create("Admin", "Admin role", false, true, organizationId);

      _userContextMock.OrganizationId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(request.RoleId, Arg.Any<CancellationToken>()).Returns(role);
      _unitOfWorkMock.Permissions.CountByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(0);
      _permissionValidatorMock.ValidateAssign(request, true, false).Returns(Result.Failure(new PermissionNotFoundInAssignmentError()));

      var result = await _permissionService.AssignToRoleAsync(request);

      Assert.False(result.IsSuccess);
      _unitOfWorkMock.Roles.DidNotReceive().Update(Arg.Any<Role>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task AssignToRoleAsync_WithUnauthorizedOrganization_ShouldReturnUnauthorized()
   {
      var request = new RolePermissionAssignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);
      var role = Role.Create("Admin", "Admin role", false, true, organizationId: Guid.NewGuid());

      _userContextMock.OrganizationId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(request.RoleId, Arg.Any<CancellationToken>()).Returns(role);

      var result = await _permissionService.AssignToRoleAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.Permissions.DidNotReceive().CountByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   #endregion

   #region UnassignFromRoleAsync Tests

   [Fact]
   public async Task UnassignFromRoleAsync_WithValidRequest_ShouldUnassignPermissionsSuccessfully()
   {
      var organizationId = Guid.NewGuid();
      var permissionId = Guid.NewGuid();
      var request = new RolePermissionUnassignRequest(RoleId: Guid.NewGuid(), PermissionIds: [permissionId]);
      var role = Role.Create("Admin", "Admin role", false, true, organizationId);
      role.AddPermission(permissionId);

      _userContextMock.OrganizationId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(request.RoleId, Arg.Any<CancellationToken>()).Returns(role);
      _permissionValidatorMock.ValidateUnassign(request, true, true).Returns(Result.Success());

      var result = await _permissionService.UnassignFromRoleAsync(request);

      Assert.True(result.IsSuccess);
      Assert.Empty(role.RolePermissions);
      _unitOfWorkMock.Roles.Received(1).Update(role);
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _rolePermissionAuthorizationCacheMock.Received(1).RemoveAsync(role.Id, Arg.Any<CancellationToken>());
      await _eventPublisherMock.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Permissions,
         IamConst.Logger.Action.Unassign,
         AuditPrivacyLevel.High,
         Arg.Any<RetentionPolicy>(), 
         Arg.Any<string>(),
         role.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UnassignFromRoleAsync_WithPermissionRoleDoesNotHave_ShouldReturnFailure()
   {
      var organizationId = Guid.NewGuid();
      var request = new RolePermissionUnassignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);
      var role = Role.Create("Admin", "Admin role", false, true, organizationId);

      _userContextMock.OrganizationId.Returns(organizationId);
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(request.RoleId, Arg.Any<CancellationToken>()).Returns(role);
      _permissionValidatorMock.ValidateUnassign(request, true, false).Returns(Result.Failure(new PermissionsCannotBeUnassignedError()));

      var result = await _permissionService.UnassignFromRoleAsync(request);

      Assert.False(result.IsSuccess);
      _unitOfWorkMock.Roles.DidNotReceive().Update(Arg.Any<Role>());
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UnassignFromRoleAsync_WithUnauthorizedOrganization_ShouldReturnUnauthorized()
   {
      var request = new RolePermissionUnassignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);
      var role = Role.Create("Admin", "Admin role", false, true, organizationId: Guid.NewGuid());

      _userContextMock.OrganizationId.Returns(Guid.NewGuid());
      _userContextMock.IsSystemAdmin.Returns(false);
      _unitOfWorkMock.Roles.GetByIdAsync(request.RoleId, Arg.Any<CancellationToken>()).Returns(role);

      var result = await _permissionService.UnassignFromRoleAsync(request);

      Assert.False(result.IsSuccess);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   #endregion

   #region GetByCodeAsync Tests

   [Fact]
   public async Task GetByCodeAsync_WithValidCode_ShouldReturnPermission()
   {
      var code = "iam.users.create";
      var permission = CreatePermissionDto("create");

      _permissionQueryRepositoryMock.GetByCodeAsync(code, Arg.Any<CancellationToken>()).Returns(permission);

      var result = await _permissionService.GetByCodeAsync(code);

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Data);
      Assert.Equal(permission, result.Data);
   }

   [Fact]
   public async Task GetByCodeAsync_WithNonExistentPermission_ShouldReturnFailure()
   {
      var code = "iam.users.create";

      _permissionQueryRepositoryMock.GetByCodeAsync(code, Arg.Any<CancellationToken>()).Returns((PermissionDto?)null);

      var result = await _permissionService.GetByCodeAsync(code);

      Assert.False(result.IsSuccess);
   }

   #endregion

   #region Cancellation Token Tests

   [Fact]
   public async Task CreateAsync_WithCancelledToken_ShouldRespectCancellation()
   {
      var cts = new CancellationTokenSource();
      cts.Cancel();
      var request = CreatePermissionCreateRequestRecord();

      _permissionQueryRepositoryMock.CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
         .Returns(Task.FromException<bool>(new OperationCanceledException()));

      await Assert.ThrowsAsync<OperationCanceledException>(() => _permissionService.CreateAsync(request, cts.Token));
   }

   #endregion

   private static PermissionCreateRequest CreatePermissionCreateRequestRecord(string module = "iam")
   {
      return new PermissionCreateRequest(
         Module: module,
         Resource: "users",
         Action: "create",
         Title: "Create Users",
         Description: "Allows creating users.");
   }

   private static PermissionUpdateRequest CreatePermissionUpdateRequestRecord()
   {
      return new PermissionUpdateRequest(
         Module: "iam",
         Resource: "users",
         Action: "update",
         Title: "Update Users",
         Description: "Allows updating users.",
         IsActive: true);
   }

   private static Permission CreatePermissionWithId(Guid id)
   {
      var permission = Permission.Create("iam", "users", "create", "Create Users", "Allows creating users.", true);
      permission.GetType().GetProperty("Id")?.SetValue(permission, id);

      return permission;
   }

   private static PermissionDto CreatePermissionDto(string name)
   {
      return new PermissionDto(
         Guid.NewGuid(),
         "iam",
         "users",
         name,
         $"iam.users.{name}",
         name,
         $"{name} permission",
         true);
   }
}
