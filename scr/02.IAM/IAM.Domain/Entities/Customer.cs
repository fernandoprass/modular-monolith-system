using IAM.Domain.Enums;
using Shared.Domain.Entities;

namespace IAM.Domain.Entities;

public class Customer : EntityAudited
{
   public CustomerType Type { get; set; }
   public string Code { get; set; }
   public string Name { get; set; }
   public string? Description { get; set; }
   public bool IsActive { get; set; } = true;
   public bool IsMaster { get; set; } = false;

   // Navigation property
   public ICollection<User> Users { get; set; } = new List<User>();

   public static Customer Create(CustomerType type, string code, string name, string? description)
   {
      return new Customer
      {
         Id = Guid.CreateVersion7(),
         Type = type,
         Code = code,
         Name = name,
         Description = description,
         IsActive = true,
         IsMaster = false
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