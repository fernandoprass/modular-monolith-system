using Asp.Versioning;
using IAM.Application.Contracts;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/authorization")]
[Authorize]
public class AuthorizationController(
   IPermissionAuthorizationService permissionAuthorizationService,
   IConfiguration configuration,
   IUserContext userContext) : BaseController(userContext)
{
   private const string InternalApiKeyHeader = "X-Internal-Api-Key";

   private readonly IPermissionAuthorizationService _permissionAuthorizationService = permissionAuthorizationService;
   private readonly IConfiguration _configuration = configuration;

   [HttpPost("check")]
   public async Task<IActionResult> CheckPermission([FromBody] PermissionCheckRequest request, CancellationToken cancellationToken)
   {
      if (!IsInternalApiKeyValid())
      {
         return Unauthorized();
      }

      var response = await _permissionAuthorizationService.CheckPermissionAsync(request, cancellationToken);
      return Ok(response);
   }

   private bool IsInternalApiKeyValid()
   {
      var expectedApiKey = _configuration["InternalApi:Key"];

      if (string.IsNullOrWhiteSpace(expectedApiKey))
      {
         return false;
      }

      return Request.Headers.TryGetValue(InternalApiKeyHeader, out var actualApiKey)
             && string.Equals(actualApiKey, expectedApiKey, StringComparison.Ordinal);
   }
}
