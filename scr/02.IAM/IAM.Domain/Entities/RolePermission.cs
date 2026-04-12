using Shared.Domain.Entities;

namespace IAM.Domain.Entities;

public class RolePermission : EntityAudited
{
   public Guid RoleId { get; private set; }
   public Guid PermissionId { get; private set; }
   public Role Role { get; private set; } = null!;
   public Permission Permission { get; private set; } = null!;

   private RolePermission() { }

   public RolePermission(Guid roleId, Guid permissionId)
   {
      Id = Guid.CreateVersion7();
      RoleId = roleId;
      PermissionId = permissionId;
   }
}
