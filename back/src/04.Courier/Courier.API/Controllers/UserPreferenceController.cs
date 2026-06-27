using Asp.Versioning;
using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Authorization;

namespace Courier.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/user-preferences")]
[Authorize]
public class UserPreferenceController(IUserPreferenceService userPreferenceService) : BaseController
{
   private readonly IUserPreferenceService _userPreferenceService = userPreferenceService;

   [HttpGet("")]
   [RequirePermission(CourierPermission.UserPreferences.Read)]
   public async Task<IActionResult> Get(CancellationToken cancellationToken)
   {
      var result = await _userPreferenceService.GetAsync(cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpPut("")]
   [RequirePermission(CourierPermission.UserPreferences.Write)]
   public async Task<IActionResult> Update(
      [FromBody] UserPreferenceUpdateRequest request,
      CancellationToken cancellationToken)
   {
      var result = await _userPreferenceService.UpdateAsync(request, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }
}
