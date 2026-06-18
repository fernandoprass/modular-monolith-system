namespace DatabaseSeeder.Interfaces
{
   public interface ISeederData
   {
      public Guid SaaSOrganizationId { get; set; }
      public Guid TestOrganizationId { get; set; }
      public Guid SysAdminRoleId { get; set; }
      public Guid OrganizationAdminRoleId { get; set; }
      public Guid UserRoleId { get; set; }
      public string SystemAdminRoleName { get; }
      public string OrganizationAdminRoleName { get; }
      public string UserRoleName { get; }
   }
}
