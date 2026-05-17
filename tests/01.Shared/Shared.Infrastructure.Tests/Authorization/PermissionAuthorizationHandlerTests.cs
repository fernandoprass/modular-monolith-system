using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Infrastructure.Authorization;
using System.Security.Claims;

namespace Shared.Infrastructure.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
   private const string RequiredPermission = "iam.users.list";

   private readonly IRolePermissionCache _permissionService;
   private readonly PermissionAuthorizationHandler _handler;

   public PermissionAuthorizationHandlerTests()
   {
      _permissionService = Substitute.For<IRolePermissionCache>();
      _handler = new PermissionAuthorizationHandler(_permissionService);
   }

   [Fact]
   public async Task HandleAsync_WhenUserIsUnauthenticated_ShouldFail()
   {
      var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity()));

      await _handler.HandleAsync(context);

      context.HasFailed.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenUserIsSystemAdmin_ShouldSucceed()
   {
      var user = CreateUser([new Claim(SharedConst.Security.Claim.IsSystemAdmin, "true")]);
      var context = CreateContext(user);

      await _handler.HandleAsync(context);

      context.HasSucceeded.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenUserHasNoRoles_ShouldFail()
   {
      var user = CreateUser([new Claim(SharedConst.Security.Claim.IsSystemAdmin, "false")]);
      var context = CreateContext(user);

      await _handler.HandleAsync(context);

      context.HasFailed.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenRoleClaimIsInvalid_ShouldFail()
   {
      var user = CreateUser(
      [
         new Claim(SharedConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(SharedConst.Security.Claim.Role, "invalid")
      ]);
      var context = CreateContext(user);

      await _handler.HandleAsync(context);

      context.HasFailed.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenRoleHasPermission_ShouldSucceed()
   {
      var roleId = Guid.NewGuid();
      _permissionService.GetPermissionsAsync(roleId.ToString(), Arg.Any<CancellationToken>())
         .Returns([RequiredPermission]);

      var context = CreateContext(CreateUser(
      [
         new Claim(SharedConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(SharedConst.Security.Claim.Role, roleId.ToString())
      ]));

      await _handler.HandleAsync(context);

      context.HasSucceeded.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenRoleLacksPermission_ShouldFail()
   {
      var roleId = Guid.NewGuid();
      _permissionService.GetPermissionsAsync(roleId.ToString(), Arg.Any<CancellationToken>())
         .Returns(["iam.organizations.list"]);

      var context = CreateContext(CreateUser(
      [
         new Claim(SharedConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(SharedConst.Security.Claim.Role, roleId.ToString())
      ]));

      await _handler.HandleAsync(context);

      context.HasFailed.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenMultipleRolesContainPermission_ShouldSucceed()
   {
      var firstRoleId = Guid.NewGuid();
      var secondRoleId = Guid.NewGuid();
      _permissionService.GetPermissionsAsync(firstRoleId.ToString(), Arg.Any<CancellationToken>())
         .Returns(["iam.organizations.list"]);
      _permissionService.GetPermissionsAsync(secondRoleId.ToString(), Arg.Any<CancellationToken>())
         .Returns([RequiredPermission]);

      var context = CreateContext(CreateUser(
      [
         new Claim(SharedConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(SharedConst.Security.Claim.Role, firstRoleId.ToString()),
         new Claim(SharedConst.Security.Claim.Role, secondRoleId.ToString())
      ]));

      await _handler.HandleAsync(context);

      context.HasSucceeded.Should().BeTrue();
   }

   private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user)
   {
      var requirement = new RequirePermissionAttribute(RequiredPermission);
      return new AuthorizationHandlerContext([requirement], user, null);
   }

   private static ClaimsPrincipal CreateUser(IEnumerable<Claim> claims)
   {
      return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
   }
}
