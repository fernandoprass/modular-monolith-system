using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Shared.Application.Contracts;
using Shared.Domain;
using System.Security.Claims;

namespace Shared.Infrastructure.Security;

public class AspNetUserContext(IHttpContextAccessor accessor) : IUserContext
{
   private readonly IHttpContextAccessor _accessor = accessor;

   public Guid UserOwnerId => GetOrganizationId();
   public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
   public bool IsSystemAdmin => GetIsSystemAdmin();
   public string? IpAddress => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
   public string? UserAgent => _accessor.HttpContext?.Request.Headers[HeaderNames.UserAgent].ToString();
   public string Language => GetLanguage();

   public Guid UserId => GetUserId();
   public IEnumerable<string> Roles => GetRoles();

   private Guid GetOrganizationId()
   {
      var value = _accessor.HttpContext?.User.FindFirst(SharedConst.Security.Claim.UserOwnerId)?.Value;
      return Guid.TryParse(value, out var id) ? id : Guid.Empty;
   }

   private bool GetIsSystemAdmin()
   {
      var value = _accessor.HttpContext?.User.FindFirst(SharedConst.Security.Claim.IsSystemAdmin)?.Value;
      return bool.TryParse(value, out var isSystemAdmin) && isSystemAdmin;
   }

   private Guid GetUserId()
   {
      var value = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? _accessor.HttpContext?.User.FindFirst("sub")?.Value;

      return Guid.TryParse(value, out var id) ? id : Guid.Empty;
   }

   private string GetLanguage()
   {
      var value = _accessor.HttpContext?.User.FindFirst(SharedConst.Security.Claim.Language)?.Value;

      return string.IsNullOrEmpty(value) ? SharedConst.System.DefaultLanguage : value;
   }

   private List<string> GetRoles()
   {
      return _accessor.HttpContext?.User.FindAll(SharedConst.Security.Claim.Role)
                          .Select(c => c.Value)
                          .ToList() ?? [];
   }
}
