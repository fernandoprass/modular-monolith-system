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
            Description: role.Description,
            IsActive: role.IsActive,
            IsDefault: role.IsDefault,
            OrganizationId: role.OrganizationId
         );
      }
   }
}
