using Asp.Versioning;
using Courier.Domain;
using IAM.Domain;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Domain;

namespace Core.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}")]
public class HealthController : ControllerBase
{
   [HttpGet("core/health")]
   public IActionResult Core()
   {
      return Ok(new { Status = "Ok", Module = CoreConst.ModuleName });
   }

   [HttpGet("iam/health")]
   public IActionResult Iam()
   {
      return Ok(new { Status = "Ok", Module = IamConst.System.ModuleName });
   }

   [HttpGet("sentinel/health")]
   public IActionResult Sentinel()
   {
      return Ok(new { Status = "Ok", Module = SentinelConst.System.ModuleName });
   }

   [HttpGet("courier/health")]
   public IActionResult Courier()
   {
      return Ok(new { Status = "Ok", Module = CourierConst.System.ModuleName });
   }
}
