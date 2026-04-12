using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;

namespace IAM.Domain.Mappers
{
   public static class PermissionMappers
   {
      public static PermissionDto ToPermissionDto(this Permission permission)
      {
         return new PermissionDto
         (
            Id: permission.Id,
            Module: permission.Module,
            Group: permission.Group,
            Name: permission.Name,
            Title: permission.Title,
            Description: permission.Description,
            IsActive: permission.IsActive
         );
      }
   }
}
