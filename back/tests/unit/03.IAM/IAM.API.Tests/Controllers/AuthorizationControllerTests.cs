using IAM.API.Controllers;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace IAM.API.Tests.Controllers;

public class AuthorizationControllerTests
{
   [Fact]
   public async Task CheckPermission_WhenInternalApiKeyIsInvalid_ReturnsUnauthorized()
   {
      var controller = CreateController("valid-key");
      controller.ControllerContext.HttpContext.Request.Headers["X-Internal-Api-Key"] = "invalid-key";

      var result = await controller.CheckPermission(new PermissionCheckRequest(IamPermission.Users.Read), CancellationToken.None);

      Assert.IsType<UnauthorizedResult>(result);
   }

   [Fact]
   public async Task CheckPermission_WhenRoleHasPermission_ReturnsAllowed()
   {
      var permissionAuthorizationService = Substitute.For<IPermissionAuthorizationService>();
      var controller = CreateController("valid-key", permissionAuthorizationService);
      controller.ControllerContext.HttpContext.Request.Headers["X-Internal-Api-Key"] = "valid-key";
      permissionAuthorizationService.CheckPermissionAsync(Arg.Any<PermissionCheckRequest>(), Arg.Any<CancellationToken>())
         .Returns(new PermissionCheckResponse(true));

      var result = await controller.CheckPermission(new PermissionCheckRequest(IamPermission.Users.Read), CancellationToken.None);

      var okResult = Assert.IsType<OkObjectResult>(result);
      var response = Assert.IsType<PermissionCheckResponse>(okResult.Value);
      Assert.True(response.Allowed);
   }

   private static AuthorizationController CreateController(
      string internalApiKey,
      IPermissionAuthorizationService? permissionAuthorizationService = null)
   {
      var configuration = new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["InternalApi:Key"] = internalApiKey
         })
         .Build();

      return new AuthorizationController(
         permissionAuthorizationService ?? Substitute.For<IPermissionAuthorizationService>(),
         configuration)
      {
         ControllerContext = new ControllerContext
         {
            HttpContext = new DefaultHttpContext()
         }
      };
   }
}
