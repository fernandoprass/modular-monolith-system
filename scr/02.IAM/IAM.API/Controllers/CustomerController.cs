using Asp.Versioning;
using IAM.Application.Contracts;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAM.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/iam/customers")]
public class CustomerController(
   ICustomerService customerService,
   IRegisterOrchestrator registerOrchestrator) : BaseController
{
   private readonly ICustomerService _customerService = customerService;
   private readonly IRegisterOrchestrator _registerOrchestrator = registerOrchestrator;

   [HttpGet("{id:guid}")]
   [Authorize]
   public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
   {
      var customer = await _customerService.GetByIdAsync(id, cancellationToken);
      return OkOrNotFound(customer);
   }

   [HttpGet()]
   [Authorize]
   public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken)
   {
      var customer = await _customerService.GetByNameAsync(name, cancellationToken);
      return OkOrNotFound(customer);
   }

   [HttpPost]
   public async Task<IActionResult> Create([FromBody] CustomerCreateRequest customer, CancellationToken cancellationToken)
   {
      var result = await _registerOrchestrator.RegisterCustomerAsync(customer, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPut("{id:guid}")]
   [Authorize]
   public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateRequest customer, CancellationToken cancellationToken)
   {
      var result = await _customerService.UpdateAsync(id, customer, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpPatch("{id:guid}/code")]
   [Authorize]
   public async Task<IActionResult> UpdateCode(Guid id, [FromBody] CustomerUpdateCodeRequest customer, CancellationToken cancellationToken)
   {
      var result = await _customerService.UpdateCodeAsync(id, customer, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpDelete("{id:guid}")]
   [Authorize]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _registerOrchestrator.DeleteCustomerAsync(id, cancellationToken);
      return OkOrNotFound(result);
   }
}