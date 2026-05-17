using IAM.Application.Services;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using NSubstitute;
using Shared.Application.Contracts;
using SharedPermissionService = Shared.Application.Contracts.IRolePermissionCache;

namespace IAM.Application.Tests.Services;

public class PermissionAuthorizationServiceTests
{
   private readonly IUserContext _userContext;
   private readonly SharedPermissionService _permissionService;
   private readonly PermissionAuthorizationService _service;

   public PermissionAuthorizationServiceTests()
   {
      _userContext = Substitute.For<IUserContext>();
      _permissionService = Substitute.For<SharedPermissionService>();

      _service = new PermissionAuthorizationService(_userContext, _permissionService);
   }

   [Fact]
   public async Task CheckPermissionAsync_WhenRoleHasPermission_ReturnsAllowed()
   {
      var roleId = Guid.NewGuid();
      _userContext.Roles.Returns([roleId.ToString()]);
      _permissionService.GetPermissionsAsync(roleId.ToString(), Arg.Any<CancellationToken>())
         .Returns([IamPermission.Users.List]);

      var result = await _service.CheckPermissionAsync(new PermissionCheckRequest(IamPermission.Users.List));

      Assert.True(result.Allowed);
   }

   [Fact]
   public async Task CheckPermissionAsync_WhenUserHasNoRoles_ReturnsDenied()
   {
      _userContext.Roles.Returns([]);

      var result = await _service.CheckPermissionAsync(new PermissionCheckRequest(IamPermission.Users.List));

      Assert.False(result.Allowed);
      await _permissionService.DidNotReceive().GetPermissionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CheckPermissionAsync_WhenCachedRoleHasPermission_ReturnsAllowed()
   {
      var roleId = Guid.NewGuid();
      _userContext.Roles.Returns([roleId.ToString()]);
      _permissionService.GetPermissionsAsync(roleId.ToString(), Arg.Any<CancellationToken>())
         .Returns([IamPermission.Users.List]);

      var result = await _service.CheckPermissionAsync(new PermissionCheckRequest(IamPermission.Users.List));

      Assert.True(result.Allowed);
   }
}
