using Microsoft.AspNetCore.Authorization;

namespace IAM.API.Middlewares;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute(string permission) : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
   public string Permission { get; } = permission;

   public IEnumerable<IAuthorizationRequirement> GetRequirements()
   {
      yield return this;
   }
}
