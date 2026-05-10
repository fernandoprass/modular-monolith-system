using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Entities;

namespace Shared.Domain.Mappers;

public static class ParameterMappers
{
   public static ParameterDto ToParameterDto(this Parameter parameter)
   {
      return new ParameterDto(
         parameter.Id,
         parameter.Module,
         parameter.Group,
         parameter.Name,
         parameter.Key,
         parameter.Title,
         parameter.Description,
         parameter.Type,
         parameter.Value,
         parameter.ListItems,
         parameter.ExternalListEndpoint,
         parameter.OverrideType,
         parameter.IsVisible
      );
   }

   public static ParameterSearchRequestInternal ToInternal(
     this ParameterSearchRequest publicRequest,
     Guid userOwnerId,
     Guid userId,
     bool isSystemAdmin)
   {
      return new ParameterSearchRequestInternal(
          publicRequest.Module,
          publicRequest.Group,
          publicRequest.Name,
          publicRequest.Key,
          publicRequest.Title,
          publicRequest.Description,
          userId,
          userOwnerId,
          isSystemAdmin
      );
   }

}
