using Microsoft.AspNetCore.Mvc;
using Myce.Response;
using Shared.Domain.Messages;

namespace Sentinel.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
   protected IActionResult OkOrNotFound<T>(T? value) where T : class
   {
      if (value == null)
      {
         return NotFound(Result<T>.Failure(new NotFoundError()));
      }

      if (value is Result result && !result.IsSuccess)
      {
         return BadRequest(result);
      }

      return value is Result ? Ok(value) : Ok(Result<T>.Success(value));
   }
}
