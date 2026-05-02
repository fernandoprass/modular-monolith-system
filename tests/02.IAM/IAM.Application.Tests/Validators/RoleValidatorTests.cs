using FluentAssertions;
using IAM.Application.Validators;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.Messages;
using Moq;
using Shared.Application.Contracts;
using Shared.Domain.Messages;

namespace IAM.Application.Tests.Validators;

public class RoleValidatorTests
{
   private readonly Mock<IUserContext> _userContextMock;
   private readonly RoleValidator _validator;

   public RoleValidatorTests()
   {
      _userContextMock = new Mock<IUserContext>();
      _validator = new RoleValidator(_userContextMock.Object);
   }

   #region ValidateCreate Tests

   [Fact]
   public void ValidateCreate_ShouldReturnSuccess_WhenRequestIsValid()
   {
      var request = new RoleCreateRequest(Name: "Admin", Description: "Administrator role", IsDefault: false, IsActive: true, OrganizationId: Guid.NewGuid());
      _userContextMock.Setup(x => x.UserOwnerId).Returns(request.OrganizationId.Value);

      var result = _validator.ValidateCreate(request, nameAlreadyExists: false);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateCreate_ShouldReturnFailure_WhenNameAlreadyExists()
   {
      var request = new RoleCreateRequest(Name: "Admin", Description: "Administrator role", IsDefault: false, IsActive: true, OrganizationId: Guid.NewGuid());

      var result = _validator.ValidateCreate(request, nameAlreadyExists: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is RoleDuplicateNameError);
   }

   [Fact]
   public void ValidateCreate_ShouldReturnFailure_WhenOrgIdDoesNotMatchUserOwnerId()
   {
      var request = new RoleCreateRequest(Name: "Admin", Description: "Administrator role", IsDefault: false, IsActive: true, OrganizationId: Guid.NewGuid());
      _userContextMock.Setup(x => x.UserOwnerId).Returns(Guid.NewGuid()); // Different ID

      var result = _validator.ValidateCreate(request, nameAlreadyExists: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is OrganizationForbiddenError);
   }

   #endregion

   #region ValidateUpdate Tests

   [Fact]
   public void ValidateUpdate_ShouldReturnFailure_WhenRoleDoesNotExist()
   {
      var request = new RoleUpdateRequest(Name: "Admin", Description: "Administrator role", IsDefault: false, IsActive: true);

      var result = _validator.ValidateUpdate(request, roleExists: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is NotFoundError);
   }

   #endregion

   #region ValidateAssign Tests

   [Fact]
   public void ValidateAssign_ShouldReturnFailure_WhenRolesAreMissingInSystem()
   {
      var request = new RoleAssignRequest(Guid.NewGuid(),
         Roles = new List<RoleAssignmentDto> ( new() { Id = Guid.NewGuid() } )
      );

      var result = _validator.ValidateAssign(request, userExists: true, allRolesAvailable: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is RolesCannotBeAssignedError);
   }

   [Fact]
   public void ValidateAssign_ShouldReturnFailure_WhenRoleHasPastExpirationDate()
   {
      var request = new RoleAssignRequest
      {
         Roles = new List<RoleAssignmentDto>
            {
                new() { Id = Guid.NewGuid(), ExpiresAt = DateTime.UtcNow.AddDays(-1) }
            }
      };

      var result = _validator.ValidateAssign(request, userExists: true, allRolesAvailable: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is RolesInvalidExpirationError);
   }

   #endregion

   #region ValidateUnassign Tests

   [Fact]
   public void ValidateUnassign_ShouldReturnSuccess_WhenUserHasAllRoles()
   {
      var request = new RoleUnassignRequest { RoleIds = new List<Guid> { Guid.NewGuid() } };

      var result = _validator.ValidateUnassign(request, userExists: true, userHasAllRoles: true);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUnassign_ShouldReturnFailure_WhenUserDoesNotHaveTheRoles()
   {
      var request = new RoleUnassignRequest { RoleIds = new List<Guid> { Guid.NewGuid() } };

      var result = _validator.ValidateUnassign(request, userExists: true, userHasAllRoles: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(e => e is RolesCannotBeUnassignedError);
   }

   #endregion
}