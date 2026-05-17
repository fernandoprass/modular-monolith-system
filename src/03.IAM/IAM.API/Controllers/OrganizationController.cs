using Asp.Versioning;
using Shared.Infrastructure.Authorization;
using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/organizations")]
public class OrganizationController(
   IOrganizationService organizationService,
   IRegisterOrchestrator registerOrchestrator) : BaseController
{
   private readonly IOrganizationService _organizationService = organizationService;
   private readonly IRegisterOrchestrator _registerOrchestrator = registerOrchestrator;

   [HttpGet("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.View)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var organization = await _organizationService.GetByIdAsync(id, cancellationToken);
      return OkOrNotFound(organization);
   }

   [HttpGet()]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.List)]
   public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken)
   {
      var organization = await _organizationService.GetByNameAsync(name, cancellationToken);
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
   [RequirePermission(IamPermission.Organizations.Update)]
   public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationUpdateRequest organization, CancellationToken cancellationToken)
   {
      var result = await _organizationService.UpdateAsync(id, organization, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPatch("{id:guid}/code")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Update)]
   public async Task<IActionResult> UpdateCode(Guid id, [FromBody] OrganizationUpdateCodeRequest organization, CancellationToken cancellationToken)
   {
      var result = await _organizationService.UpdateCodeAsync(id, organization, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   [RequirePermission(IamPermission.Organizations.Delete)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _registerOrchestrator.DeleteOrganizationAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }
}

