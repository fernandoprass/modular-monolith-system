using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;

namespace IAM.Domain.Mappers;

public static class OrganizationMappers
{
   public static OrganizationDto ToOrganizationDto(this Organization organization)
   {
      return new OrganizationDto
      (
         Id: organization.Id,
         Type: organization.Type,
         Code: organization.Code,
         Name: organization.Name,
         Description: organization.Description,
         DefaultLanguage: organization.DefaultLanguage,
         IsActive: organization.IsActive
      );
   }
}
