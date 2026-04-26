using Microsoft.AspNetCore.Authorization;

namespace IAM.API.Middlewares;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationRequirement
{
   public string Permission { get; }

   public RequirePermissionAttribute(string permission)
   {
      Permission = permission;
   }
}