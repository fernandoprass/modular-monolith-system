using IAM.Domain.Enums;
using Shared.Domain.Entities;

namespace IAM.Domain.Entities;

public class Organization : EntityAudited
{
   public OrganizationType Type { get; set; }
   public string Code { get; set; }
   public string Name { get; set; }
   public string? Description { get; set; }
   public bool IsActive { get; set; } = true;

   // Navigation property
   public ICollection<User> Users { get; set; } = new List<User>();

   public ICollection<Role> Roles { get; set; } = new List<Role>();

   public static Organization Create(OrganizationType type, string code, string name, string? description)
   {
      return new Organization
      {
         Id = Guid.CreateVersion7(),
         Type = type,
         Code = code,
         Name = name,
         Description = description,
         IsActive = true,
      };
   }

   public void Update(string code)
   {
      Code = code;
   }

   public void Update(string name, string? description, bool isActive)
   {
      Name = name;
      Description = description;
      IsActive = isActive;
   }
}