using Asp.Versioning;
using Shared.Infrastructure.Authorization;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/users")]
public class UserController(
   IUserContext userContext,
   IRegisterOrchestrator registerOrchestrator,
   IUserService userService,
   IAuthService authService) : BaseController
{
   private readonly IUserContext _userContext = userContext;
   private readonly IRegisterOrchestrator _registerOrchestrator = registerOrchestrator;
   private readonly IUserService _userService = userService;
   private readonly IAuthService _authService = authService;

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Users.Read)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var user = await _userService.GetByIdAsync(id, cancellationToken);
      return OkOrNotFound(user);
   }

   [HttpGet("")]
   [Authorize]
   [RequirePermission(IamPermission.Users.Read)]
   public async Task<IActionResult> GetByOrganizationId([FromQuery] UserSearchRequest request, CancellationToken cancellationToken)
   {
      var users = await _userService.GetAsync(request, cancellationToken);

      return OkOrNotFound(users);
   }

   [HttpGet("lookup")]
   [Authorize]
   [RequirePermission(IamPermission.Users.Read)]
   public async Task<IActionResult> GetLookup([FromQuery] UserLookupRequest request, CancellationToken cancellationToken)
   {
      var users = await _userService.GetLookupAsync(request, cancellationToken);

      return OkOrNotFound(users);
   }

   [HttpPost("")]
   [Authorize]
   [RequirePermission(IamPermission.Users.Write)]
   public async Task<IActionResult> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
   {
      var user = await _registerOrchestrator.RegisterUserAsync(request, cancellationToken);
      return OkOrNotFound(user);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Users.Write)]
   public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
   {
      var response = await _userService.UpdateAsync(id, request, cancellationToken);

      return OkOrNotFound(response);
   }

   [HttpPatch("{id:guid}/organization-admin")]
   [Authorize]
   [RequirePermission(IamPermission.Users.UpdateOrganizationAdmin)]
   public async Task<IActionResult> UpdateOrganizationAdmin(Guid id, [FromBody] UserUpdateOrganizationAdminRequest request, CancellationToken cancellationToken)
   {
      var result = await _userService.UpdateOrganizationAdminAsync(id, request, cancellationToken);

      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Users.Write)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var response = await _userService.DeleteAsync(id, cancellationToken);

      return OkOrNotFound(response);
   }

   [HttpGet("profile")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.Read)]
   public async Task<IActionResult> GetByCurrentUser(CancellationToken cancellationToken)
   {
      var user = await _userService.GetByIdAsync(_userContext.UserId, cancellationToken);
      return OkOrNotFound(user);
   }

   [HttpPut("profile")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.Write)]
   public async Task<IActionResult> UpdateMe([FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
   {
      var response = await _userService.UpdateMeAsync(request, cancellationToken);

      return OkOrNotFound(response);
   }

   [HttpPatch("profile/password")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.Write)]
   public async Task<IActionResult> UpdatePassword([FromBody] UserUpdatePasswordRequest request, CancellationToken cancellationToken)
   {
      var result = await _userService.UpdatePasswordAsync(request, cancellationToken);

      return OkOrNotFound(result);
   }

   [HttpDelete("profile")]
   [Authorize]
   [RequirePermission(IamPermission.UserProfile.Delete)]
   public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
   {
      var response = await _userService.DeleteMeAsync(cancellationToken);

      return OkOrNotFound(response);
   }

   [HttpPost("login")]
   public async Task<IActionResult> Login([FromBody] UserLoginRequest request, CancellationToken cancellationToken)
   {
      var response = await _authService.LoginAsync(request, cancellationToken);

      return response.IsSuccess ? OkOrNotFound(response) : Unauthorized(response);
   }
}

