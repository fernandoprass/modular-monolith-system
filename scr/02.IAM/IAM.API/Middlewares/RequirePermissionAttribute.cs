using Microsoft.AspNetCore.Authorization;

namespace IAM.API.Middlewares;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute(string permission) : Attribute, IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}