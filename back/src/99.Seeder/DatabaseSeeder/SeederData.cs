using DatabaseSeeder.Interfaces;

namespace DatabaseSeeder
{
   public class SeederData : ISeederData
   {
      public Guid SaaSOrganizationId { set; get; }
      public Guid TestOrganizationId { set; get; }
      public Guid SysAdminRoleId { set; get; }
      public Guid OrganizationAdminRoleId { set; get; }
      public Guid UserRoleId { set; get; }
      string ISeederData.SystemAdminRoleName { get => "System Admin";  }
      string ISeederData.OrganizationAdminRoleName { get => "Organization Admin"; }
      string ISeederData.UserRoleName { get => "User"; }
   }
}
