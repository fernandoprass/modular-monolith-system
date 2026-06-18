using Shared.Domain.Entities;

namespace IAM.Domain.Entities;

public class UserRole : EntityAudited
{
   public Guid UserId { get; private set; }
   public Guid RoleId { get; private set; }
   public DateTime StartsAt { get; private set; }
   public DateTime? ExpiresAt { get; private set; }
   public User User { get; private set; } = null!;
   public Role Role { get; private set; } = null!;

   private UserRole() { }

   public static UserRole Create(Guid userId, Guid roleId, DateTime startsAt, DateTime? expiresAt)
   {
      return new UserRole
      {
         Id = Guid.CreateVersion7(),
         UserId = userId,
         RoleId = roleId,
         StartsAt = startsAt,
         ExpiresAt = expiresAt
      };
   }

   public void UpdateValidity(DateTime startsAt, DateTime? expiresAt)
   {
      StartsAt = startsAt;
      ExpiresAt = expiresAt;
   }
}
