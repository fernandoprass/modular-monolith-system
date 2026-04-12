using Shared.Domain.Entities;

namespace IAM.Domain.Entities;

public class Permission : EntityAudited
{
   public string Module { get; private set; } // e.g., "IAM", "Sentinel"
   public string Group { get; private set; } // e.g., "Users"
   public string Name { get; private set; } // e.g., "user.create"
   public string Title { get; private set; }
   public string Description { get; private set; }
   public bool IsActive { get; private set; }

   private readonly List<RolePermission> _roleFeatures = new();
   public IReadOnlyCollection<RolePermission> RoleFeatures => _roleFeatures.AsReadOnly();


   public Permission(string module, string group, string name, string title, string description, bool isActive)
   {
      Id = Guid.CreateVersion7();
      Module = module;
      Group = group;
      Name = name;
      Title = title;
      Description = description;
      IsActive = isActive;
   }

   public void Update(string module, string group, string name, string title, string description, bool isActive)
   {
      Module = module;
      Group = group;
      Name = name;
      Title = title;
      Description = description;
      IsActive = isActive;
   }
}
