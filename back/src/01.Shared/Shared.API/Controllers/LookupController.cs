using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Infrastructure.Authorization;

namespace Shared.API.Controllers;

[Authorize]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/shared/lookups")]
public class LookupController(ILookupService service, IUserContext userContext) : BaseController(userContext)
{
   [HttpGet]
   [RequirePermission(SharedPermission.Lookups.Read)]
   public async Task<IActionResult> Get([FromQuery] LookupSearchRequest request, CancellationToken cancellationToken = default) =>
      OkOrNotFound(await service.GetAsync(request, cancellationToken));

   [HttpGet("{id:guid}")]
   [RequirePermission(SharedPermission.Lookups.Read)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default) =>
      OkOrNotFound(await service.GetByIdAsync(id, cancellationToken));

   [HttpPost]
   [RequirePermission(SharedPermission.Lookups.Create)]
   public async Task<IActionResult> Create([FromBody] LookupCreateRequest request, CancellationToken cancellationToken = default)
   {
      var result = await service.CreateAsync(request, cancellationToken);
      return result.HasError ? OkOrNotFound(result) : CreatedAtAction(nameof(GetById),
         new { id = result.Data!.Id, version = "1" }, Translate(result));
   }

   [HttpPut("{id:guid}")]
   [RequirePermission(SharedPermission.Lookups.Update)]
   public async Task<IActionResult> Update(Guid id, [FromBody] LookupUpdateRequest request, CancellationToken cancellationToken = default) =>
      OkOrNotFound(await service.UpdateAsync(id, request, cancellationToken));

   [HttpDelete("{id:guid}")]
   [RequirePermission(SharedPermission.Lookups.Delete)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default) =>
      OkOrNotFound(await service.DeleteAsync(id, cancellationToken));
}
