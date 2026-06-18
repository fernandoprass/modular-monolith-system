using Asp.Versioning;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;
using Shared.Infrastructure.Authorization;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/organizations")]
public class OrganizationController(
   IOrganizationService organizationService,
   IRegisterOrchestrator registerOrchestrator,
   IUserContext userContext) : BaseController
{
   private readonly IOrganizationService _organizationService = organizationService;
   private readonly IRegisterOrchestrator _registerOrchestrator = registerOrchestrator;
   private readonly IUserContext _userContext = userContext;

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Read)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var organization = await _organizationService.GetByIdAsync(id, cancellationToken);
      return OkOrNotFound(organization);
   }

   [HttpGet()]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Read)]
   public async Task<IActionResult> Get([FromQuery] OrganizationSearchRequest request, CancellationToken cancellationToken)
   {
      var organization = await _organizationService.GetAsync(request, cancellationToken);
      return OkOrNotFound(organization);
   }

   [HttpGet("lookup")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Read)]
   public async Task<IActionResult> GetLookup([FromQuery] OrganizationLookupRequest request, CancellationToken cancellationToken)
   {
      var organization = await _organizationService.GetLookupAsync(request, cancellationToken);
      return OkOrNotFound(organization);
   }

   [HttpPost]
   public async Task<IActionResult> Create([FromBody] OrganizationCreateRequest organization, CancellationToken cancellationToken)
   {
      var result = await _registerOrchestrator.RegisterOrganizationAsync(organization, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Write)]
   public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationUpdateRequest organization, CancellationToken cancellationToken)
   {
      var result = await _organizationService.UpdateAsync(id, organization, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPatch("{id:guid}/code")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Write)]
   public async Task<IActionResult> UpdateCode(Guid id, [FromBody] OrganizationUpdateCodeRequest organization, CancellationToken cancellationToken)
   {
      var result = await _organizationService.UpdateCodeAsync(id, organization, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Write)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _registerOrchestrator.DeleteOrganizationAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }


   [HttpGet("profile")]
   [Authorize]
   [RequirePermission(IamPermission.OrganizationProfile.Read)]
   public async Task<IActionResult> GetById(CancellationToken cancellationToken)
   {
      var organization = await _organizationService.GetByIdAsync(_userContext.OrganizationId, cancellationToken);
      return OkOrNotFound(organization);
   }

   [HttpPut("profile")]
   [Authorize]
   [RequirePermission(IamPermission.OrganizationProfile.Write)]
   public async Task<IActionResult> Update([FromBody] OrganizationUpdateRequest organization, CancellationToken cancellationToken)
   {
      var result = await _organizationService.UpdateAsync(_userContext.OrganizationId, organization, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("profile")]
   [Authorize]
   [RequirePermission(IamPermission.OrganizationProfile.Delete)]
   public async Task<IActionResult> Delete(CancellationToken cancellationToken)
   {
      var result = await _registerOrchestrator.DeleteOrganizationAsync(_userContext.OrganizationId, cancellationToken);
      return OkOrNotFound(result);
   }
}

