using FluentAssertions;
using IAM.API.Middlewares;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace IAM.API.Tests.Middlewares;

public class PermissionAuthorizationHandlerTests
{
   private readonly IServiceProvider _serviceProvider;
   private readonly IServiceProvider _scopeServiceProvider;
   private readonly IServiceScope _scope;
   private readonly IServiceScopeFactory _scopeFactory;
   private readonly IRolePermissionAuthorizationCache _cache;
   private readonly IRoleService _roleService;
   private readonly PermissionAuthorizationHandler _handler;

   public PermissionAuthorizationHandlerTests()
   {
      _serviceProvider = Substitute.For<IServiceProvider>();
      _scopeServiceProvider = Substitute.For<IServiceProvider>();
      _scope = Substitute.For<IServiceScope>();
      _scopeFactory = Substitute.For<IServiceScopeFactory>();
      _cache = Substitute.For<IRolePermissionAuthorizationCache>();
      _roleService = Substitute.For<IRoleService>();

      _scope.ServiceProvider.Returns(_scopeServiceProvider);
      _scopeFactory.CreateScope().Returns(_scope);
      _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_scopeFactory);
      _scopeServiceProvider.GetService(typeof(IRoleService)).Returns(_roleService);

      _handler = new PermissionAuthorizationHandler(_serviceProvider, _cache);
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
      await _roleService.DidNotReceive().GetPermissionsByRoleIdAsync(roleId);
   }

   private void ConfigureCacheMiss(Guid roleId, IEnumerable<string> permissionCodes)
   {
      _cache.GetOrCreateAsync(roleId, Arg.Any<Func<Task<IEnumerable<string>>>>())
         .Returns(call => call.Arg<Func<Task<IEnumerable<string>>>>()());

      _roleService.GetPermissionsByRoleIdAsync(roleId)
         .Returns(permissionCodes.Select(CreatePermissionDto));
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

   private static PermissionDto CreatePermissionDto(string code)
   {
      var parts = code.Split('.');
      return new PermissionDto(
         Guid.NewGuid(),
         parts[0],
         parts[1],
         parts[2],
         code,
         code,
         code,
         true);
   }
}
