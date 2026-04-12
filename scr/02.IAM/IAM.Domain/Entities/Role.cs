using Shared.Domain.Entities;

namespace IAM.Domain.Entities;

public class Role : EntityAudited
{
   public string Name { get; private set; }
   public string Description { get; private set; }
   public bool IsDefault { get; private set; } = false; // Indicates if this is a default role assigned to new users
   public bool IsActive { get; private set; } = true;
   public Guid? CustomerId { get; private set; } // Roles can be global or specific to a Customer

   private readonly List<RolePermission> _rolePermissions = new();
   public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

   private readonly List<UserRole> _userRoles = new();
   public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

   private Role() { }

   public static Role Create(string name, string description, bool isDefault, bool isActive, Guid? customerId)
   {
      return new Role
      {
         Id = Guid.CreateVersion7(),
         Name = name,
         Description = description,
         IsDefault = isDefault,
         IsActive = isActive,
         CustomerId = customerId
      };
   }

   public void Update(string name, string description, bool isDefault, bool isActive)
   {
      Name = name;
      Description = description; 
      IsDefault = isDefault;
      IsActive = isActive;
   }

   public void AddFeature(Guid permissionId)
   {
      if (!_rolePermissions.Any(rf => rf.PermissionId == permissionId))
      {
         _rolePermissions.Add(new RolePermission(Id, permissionId));
      }
   }

   public void RemoveFeature(Guid permissionId)
   {
       var feature = _rolePermissions.FirstOrDefault(rf => rf.PermissionId == permissionId);
       if (feature != null)
       {
           _rolePermissions.Remove(feature);
       }
   }

   public void ClearFeatures() => _rolePermissions.Clear();
}
