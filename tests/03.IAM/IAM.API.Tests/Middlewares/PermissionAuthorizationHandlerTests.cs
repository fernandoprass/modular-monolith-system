using FluentAssertions;
using Shared.Application.Contracts;
using Shared.Infrastructure.Authorization;
using IAM.Domain;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace IAM.API.Tests.Middlewares;

public class PermissionAuthorizationHandlerTests
{
   private readonly IRolePermissionAuthorizationCache _cache;
   private readonly IRolePermissionProvider _rolePermissionProvider;
   private readonly PermissionAuthorizationHandler _handler;

   public PermissionAuthorizationHandlerTests()
   {
      _cache = Substitute.For<IRolePermissionAuthorizationCache>();
      _rolePermissionProvider = Substitute.For<IRolePermissionProvider>();

      _handler = new PermissionAuthorizationHandler(_cache, _rolePermissionProvider);
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
      var user = CreateUser([new Claim(IamConst.Security.Claim.IsSystemAdmin, "true")]);
      var context = CreateContext(user);

      await _handler.HandleAsync(context);

      context.HasSucceeded.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenUserHasNoRoles_ShouldFail()
   {
      var user = CreateUser([new Claim(IamConst.Security.Claim.IsSystemAdmin, "false")]);
      var context = CreateContext(user);

      await _handler.HandleAsync(context);

      context.HasFailed.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenRoleClaimIsInvalid_ShouldFail()
   {
      var user = CreateUser(
      [
         new Claim(IamConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(IamConst.Security.Claim.Role, "invalid")
      ]);
      var context = CreateContext(user);

      await _handler.HandleAsync(context);

      context.HasFailed.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenRoleHasPermission_ShouldSucceed()
   {
      var roleId = Guid.NewGuid();
      ConfigureCacheMiss(roleId, [IamPermission.Users.List]);

      var context = CreateContext(CreateUser(
      [
         new Claim(IamConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(IamConst.Security.Claim.Role, roleId.ToString())
      ]));

      await _handler.HandleAsync(context);

      context.HasSucceeded.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenRoleLacksPermission_ShouldFail()
   {
      var roleId = Guid.NewGuid();
      ConfigureCacheMiss(roleId, [IamPermission.Organizations.List]);

      var context = CreateContext(CreateUser(
      [
         new Claim(IamConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(IamConst.Security.Claim.Role, roleId.ToString())
      ]));

      await _handler.HandleAsync(context);

      context.HasFailed.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenMultipleRolesContainPermission_ShouldSucceed()
   {
      var firstRoleId = Guid.NewGuid();
      var secondRoleId = Guid.NewGuid();
      ConfigureCacheMiss(firstRoleId, [IamPermission.Organizations.List]);
      ConfigureCacheMiss(secondRoleId, [IamPermission.Users.List]);

      var context = CreateContext(CreateUser(
      [
         new Claim(IamConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(IamConst.Security.Claim.Role, firstRoleId.ToString()),
         new Claim(IamConst.Security.Claim.Role, secondRoleId.ToString())
      ]));

      await _handler.HandleAsync(context);

      context.HasSucceeded.Should().BeTrue();
   }

   [Fact]
   public async Task HandleAsync_WhenCacheHasPermissions_ShouldNotCallRoleService()
   {
      var roleId = Guid.NewGuid();
      _cache.GetOrCreateAsync(roleId, Arg.Any<Func<Task<IEnumerable<string>>>>())
         .Returns([IamPermission.Users.List]);

      var context = CreateContext(CreateUser(
      [
         new Claim(IamConst.Security.Claim.IsSystemAdmin, "false"),
         new Claim(IamConst.Security.Claim.Role, roleId.ToString())
      ]));

      await _handler.HandleAsync(context);

      context.HasSucceeded.Should().BeTrue();
      await _rolePermissionProvider.DidNotReceive().GetPermissionsByRoleIdAsync(roleId);
   }

   private void ConfigureCacheMiss(Guid roleId, IEnumerable<string> permissionCodes)
   {
      _cache.GetOrCreateAsync(roleId, Arg.Any<Func<Task<IEnumerable<string>>>>())
         .Returns(call => call.Arg<Func<Task<IEnumerable<string>>>>()());

      _rolePermissionProvider.GetPermissionsByRoleIdAsync(roleId)
         .Returns(permissionCodes);
   }

   private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user)
   {
      var requirement = new RequirePermissionAttribute(IamPermission.Users.List);
      return new AuthorizationHandlerContext([requirement], user, null);
   }

   private static ClaimsPrincipal CreateUser(IEnumerable<Claim> claims)
   {
      return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
   }
}

