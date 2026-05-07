using Shared.Domain.Entities;

namespace IAM.Domain.Entities;

public class Permission : EntityAudited
{
   public string Module { get; private set; } // e.g., "IAM", "Sentinel"
   public string Resource { get; private set; } // e.g., "Users", "Parameters"
   public string Action { get; private set; } // e.g., "create", "view"
   public string Code { get; private set; }
   public string Title { get; private set; }
   public string Description { get; private set; }
   public bool IsActive { get; private set; }

   private readonly List<RolePermission> _rolePermissions = new();
   public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();


   private Permission()
   {
   }

   public static Permission Create(string module, string resource, string action, string title, string description, bool isActive)
   {
      return new Permission
      {
         Id = Guid.CreateVersion7(),
         Module = module,
         Resource = resource,
         Action = action,
         Code = BuildCode(module, resource, action),
         Title = title,
         Description = description,
         IsActive = isActive
      };
   }

   public void Update(string module, string resource, string action, string title, string description, bool isActive)
   {
      Module = module;
      Resource = resource;
      Action = action;
      Code = BuildCode(module, resource, action);
      Title = title;
      Description = description;
      IsActive = isActive;
   }

   public static string BuildCode(string module, string resource, string action)
   {
      return $"{module}.{resource}.{action}".ToLowerInvariant();
   }
}
