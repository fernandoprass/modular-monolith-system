using FluentAssertions;
using IAM.Application.Validators;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Messages;
using Myce.FluentValidator.ErrorMessages;
using Shared.Domain.Messages;

namespace IAM.Application.Tests.Validators;

public class PermissionValidatorTests
{
   private readonly PermissionValidator _validator = new();

   #region ValidateCreate Tests

   [Fact]
   public void ValidateCreate_ShouldReturnSuccess_WhenRequestIsValid()
   {
      var request = GetCreateRequest();

      var result = _validator.ValidateCreate(request, codeAlreadyExists: false);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateCreate_ShouldReturnFailure_WhenCodeAlreadyExists()
   {
      var request = GetCreateRequest();

      var result = _validator.ValidateCreate(request, codeAlreadyExists: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is PermissionDuplicateError);
   }

   [Fact]
   public void ValidateCreate_ShouldReturnFailure_WhenModuleIsInvalid()
   {
      var request = GetCreateRequest(module: "i");

      var result = _validator.ValidateCreate(request, codeAlreadyExists: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().NotBeEmpty();
   }

   #endregion

   #region ValidateUpdate Tests

   [Fact]
   public void ValidateUpdate_ShouldReturnSuccess_WhenRequestIsValid()
   {
      var request = GetUpdateRequest();

      var result = _validator.ValidateUpdate(request, codeAlreadyExists: false, permissionExists: true);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUpdate_ShouldReturnFailure_WhenPermissionDoesNotExist()
   {
      var request = GetUpdateRequest();

      var result = _validator.ValidateUpdate(request, codeAlreadyExists: false, permissionExists: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is NotFoundError);
   }

   [Fact]
   public void ValidateUpdate_ShouldReturnFailure_WhenCodeAlreadyExists()
   {
      var request = GetUpdateRequest();

      var result = _validator.ValidateUpdate(request, codeAlreadyExists: true, permissionExists: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is PermissionDuplicateError);
   }

   #endregion

   #region ValidateAssign Tests

   [Fact]
   public void ValidateAssign_ShouldReturnSuccess_WhenRequestIsValid()
   {
      var request = new RolePermissionAssignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);

      var result = _validator.ValidateAssign(request, roleExists: true, allPermissionsExist: true);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateAssign_ShouldReturnFailure_WhenRoleDoesNotExist()
   {
      var request = new RolePermissionAssignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);

      var result = _validator.ValidateAssign(request, roleExists: false, allPermissionsExist: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is NotFoundError);
   }

   [Fact]
   public void ValidateAssign_ShouldReturnFailure_WhenPermissionsAreMissingInSystem()
   {
      var request = new RolePermissionAssignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);

      var result = _validator.ValidateAssign(request, roleExists: true, allPermissionsExist: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is PermissionNotFoundInAssignmentError);
   }

   [Fact]
   public void ValidateAssign_ShouldReturnFailure_WhenHasDuplicatePermissions()
   {
      var permissionId = Guid.NewGuid();
      var request = new RolePermissionAssignRequest(RoleId: Guid.NewGuid(), PermissionIds: [permissionId, permissionId]);

      var result = _validator.ValidateAssign(request, roleExists: true, allPermissionsExist: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is ContainsDuplicateItemsError);
   }

   #endregion

   #region ValidateUnassign Tests

   [Fact]
   public void ValidateUnassign_ShouldReturnSuccess_WhenRoleHasAllPermissions()
   {
      var request = new RolePermissionUnassignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);

      var result = _validator.ValidateUnassign(request, roleExists: true, roleHasAllPermissions: true);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUnassign_ShouldReturnFailure_WhenRoleDoesNotHavePermissions()
   {
      var request = new RolePermissionUnassignRequest(RoleId: Guid.NewGuid(), PermissionIds: [Guid.NewGuid()]);

      var result = _validator.ValidateUnassign(request, roleExists: true, roleHasAllPermissions: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is PermissionsCannotBeUnassignedError);
   }

   [Fact]
   public void ValidateUnassign_ShouldReturnFailure_WhenHasDuplicatePermissions()
   {
      var permissionId = Guid.NewGuid();
      var request = new RolePermissionUnassignRequest(RoleId: Guid.NewGuid(), PermissionIds: [permissionId, permissionId]);

      var result = _validator.ValidateUnassign(request, roleExists: true, roleHasAllPermissions: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is ContainsDuplicateItemsError);
   }

   #endregion

   private static PermissionCreateRequest GetCreateRequest(string module = "iam")
   {
      return new PermissionCreateRequest(
         Module: module,
         Resource: "users",
         Action: "create",
         Title: "Create Users",
         Description: "Allows creating users.");
   }

   private static PermissionUpdateRequest GetUpdateRequest()
   {
      return new PermissionUpdateRequest(
         Module: "iam",
         Resource: "users",
         Action: "create",
         Title: "Create Users",
         Description: "Allows creating users.",
         IsActive: true);
   }
}
