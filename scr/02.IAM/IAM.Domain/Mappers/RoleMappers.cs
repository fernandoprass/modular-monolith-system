using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;

namespace IAM.Domain.Mappers
{
   public static class RoleMappers
   {
      public static RoleDto ToRoleDto(this Role role)
      {
         return new RoleDto
         (
            Id: role.Id,
            Name: role.Name,
            IsActive: role.IsActive,
            IsDefault: role.IsDefault,
            OrganizationId: role.OrganizationId,
            Features: role.RolePermissions.Select(rp => rp.Permission.ToPermissionDto())
         );
      }
   }
}
