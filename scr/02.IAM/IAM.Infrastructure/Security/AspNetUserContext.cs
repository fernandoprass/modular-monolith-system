using IAM.Domain;
using Microsoft.AspNetCore.Http;
using Shared.Application.Contracts;
using System.Security.Claims;

namespace IAM.Infrastructure.Security;

public class AspNetUserContext(IHttpContextAccessor accessor) : IUserContext
{
   private readonly IHttpContextAccessor _accessor = accessor;

   public Guid UserOwnerId => GetOrganizationId();
   public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
   public bool IsSystemAdmin => GetIsSystemAdmin();
   public Guid UserId => GetUserId();

   private Guid GetOrganizationId()
   {
      var value = _accessor.HttpContext?.User.FindFirst(IamConst.Security.Claim.UserOwnerId)?.Value;
      return Guid.TryParse(value, out var id) ? id : Guid.Empty;
   }

   private bool GetIsSystemAdmin()
   {
      var value = _accessor.HttpContext?.User.FindFirst(IamConst.Security.Claim.IsSystemAdmin)?.Value;
      return Boolean.TryParse(value, out var isSystemAdmin) ? isSystemAdmin : false;
   }

   private Guid GetUserId()
   {
      var value = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? _accessor.HttpContext?.User.FindFirst("sub")?.Value;

      return Guid.TryParse(value, out var id) ? id : Guid.Empty;
   }

   private bool? IsInRole(string role)
   {
      return _accessor.HttpContext?.User.HasClaim(ClaimTypes.Role, role);
   }

}