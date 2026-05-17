using Microsoft.AspNetCore.Authorization;

namespace Shared.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute(string permission) : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
   public string Permission { get; } = permission;

   public IEnumerable<IAuthorizationRequirement> GetRequirements()
   {
      yield return this;
   }
}
