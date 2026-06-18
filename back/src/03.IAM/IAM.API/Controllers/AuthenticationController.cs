using Asp.Versioning;
using IAM.Application.Contracts;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/authentication")]
public class AuthenticationController(IAuthService authService) : BaseController
{
   private readonly IAuthService _authService = authService;

   [HttpPost("login")]
   public async Task<IActionResult> Login([FromBody] UserLoginRequest request, CancellationToken cancellationToken)
   {
      var response = await _authService.LoginAsync(request, cancellationToken);

      return response.IsSuccess ? OkOrNotFound(response) : Unauthorized(response);
   }
}
