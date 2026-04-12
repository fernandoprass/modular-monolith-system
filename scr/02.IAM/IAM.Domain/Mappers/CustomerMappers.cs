using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;

namespace IAM.Domain.Mappers;

public static class CustomerMappers
{
   public static CustomerDto ToCustomerDto(this Customer customer)
   {
      return new CustomerDto
      (
         Id : customer.Id,
         Type : customer.Type,
         Code : customer.Code,
         Name : customer.Name,
         Description : customer.Description,
         IsActive : customer.IsActive
      );
   }
}
