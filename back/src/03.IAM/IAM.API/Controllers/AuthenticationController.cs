using Asp.Versioning;
using IAM.Application.Contracts;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/authentication")]
public class AuthenticationController(
   IAuthService authService,
   IUserContext userContext) : BaseController(userContext)
{
   private readonly IAuthService _authService = authService;

   [HttpPost("login")]
   public async Task<IActionResult> Login([FromBody] UserLoginRequest request, CancellationToken cancellationToken)
   {
      var response = await _authService.LoginAsync(request, cancellationToken);

      return response.IsSuccess ? OkOrNotFound(response) : Unauthorized(Translate(response));
   }
}
