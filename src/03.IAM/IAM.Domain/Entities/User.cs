using Shared.Domain.Entities;

namespace IAM.Domain.Entities;
public class User : EntityAudited
{
   public string Name { get; set; } = string.Empty;
   public string Email { get; set; } = string.Empty;
   public string PasswordHash { get; set; } = string.Empty;
   public bool IsActive { get; set; } = true;
   public bool IsSystemAdmin { get; set; } = false;
   public bool IsOrganizationAdmin { get; set; } = false;
   public int NumFailedLoginAttempts { get; set; } = 0;
   public DateTime? EmailVerifiedAt { get; set; }
   public DateTime? LastLoginAt { get; set; }
   public DateTime? PasswordExpiresAt { get; set; }
   public DateTime? LockedOutUntil { get; set; }
   public Guid OrganizationId { get; set; }
   public Organization Organization { get; set; } = null!;

   private readonly List<UserRole> _userRoles = new();
   public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

   private User() { }

   public static User Create(string name, string email, string passwordHash, DateTime passwordExpiresAt, Guid organizationId)
   {
      return new User
      {
         Id = Guid.CreateVersion7(),
         Name = name,
         Email = email.ToLower().Trim(),
         PasswordHash = passwordHash,
         PasswordExpiresAt = passwordExpiresAt,
         IsActive = true,
         IsSystemAdmin = false,
         IsOrganizationAdmin = false,
         NumFailedLoginAttempts = 0,
         OrganizationId = organizationId
      };
   }
   public static User CreateOrganizationAdmin(string name, string email, string passwordHash, DateTime passwordExpiresAt, bool isOrganizationAdmin, Guid organizationId)
   {
      var user = User.Create(name, email, passwordHash, passwordExpiresAt, organizationId);

      user.IsOrganizationAdmin = isOrganizationAdmin;

      return user;
   }
   public void Update(string name, bool isActive)
   {
      Name = name;
      IsActive = isActive;
   }

   public void UpdatePassword(string newPasswordHash, DateTime expiresAt)
   {
      PasswordHash = newPasswordHash;
      UpdatedAt = DateTime.UtcNow;
      PasswordExpiresAt = expiresAt;
   }

   public void RegisterLastSuccessfullyLogin()
   {
      LastLoginAt = DateTime.UtcNow;
      NumFailedLoginAttempts = 0;
      LockedOutUntil = null;
   }

   public void RegisterFailedLoginAttempt(int maxFailedAttempts, int lockoutMinutes)
   {
      NumFailedLoginAttempts++;

      if (NumFailedLoginAttempts >= maxFailedAttempts)
      {
         LockedOutUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
      }
   }

   public void AddRole(Guid roleId, DateTime? expiresAt)
   {
      if (!_userRoles.Any(ur => ur.RoleId == roleId))
      {
         _userRoles.Add(UserRole.Create(Id, roleId, expiresAt));
      }
   }

   public void RemoveRole(Guid roleId)
   {
      var role = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
      if (role != null)
      {
         _userRoles.Remove(role);
      }
   }

   public void ClearRoles() => _userRoles.Clear();
}