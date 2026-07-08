using Microsoft.AspNetCore.Mvc;
using Myce.Response;
using Myce.Response.Messages;
using Shared.Application.Contracts;
using Shared.Domain.Messages;

namespace IAM.API.Controllers;

[ApiController]
public abstract class BaseController(IUserContext userContext) : ControllerBase
{
   private readonly IUserContext _userContext = userContext;

   protected IActionResult OkOrNotFound<T>(Result<T>? result)
   {
      if (result == null)
      {
         return NotFound(Translate(Result<T>.Failure(new NotFoundError())));
      }

      var translatedResult = Translate(result);
      return translatedResult.HasError ? BadRequest(translatedResult) : Ok(translatedResult);
   }

   protected IActionResult OkOrNotFound(Result? result)
   {
      if (result == null)
      {
         return NotFound(Translate(Result.Failure(new NotFoundError())));
      }

      var translatedResult = Translate(result);
      return translatedResult.HasError ? BadRequest(translatedResult) : Ok(translatedResult);
   }

   protected IActionResult OkOrNotFound<T>(T? value) where T : class
   {
      if (value == null)
      {
         return NotFound(Translate(Result<T>.Failure(new NotFoundError())));
      }

      return Ok(Result<T>.Success(value));
   }

   protected Result Translate(Result result)
   {
      return result.HasMessage
         ? new Result(result.Messages.WithLanguage(_userContext.Language))
         : result;
   }

   protected Result<T> Translate<T>(Result<T> result)
   {
      return result.HasMessage
         ? new Result<T>(result.Data, result.Messages.WithLanguage(_userContext.Language))
         : result;
   }
}
