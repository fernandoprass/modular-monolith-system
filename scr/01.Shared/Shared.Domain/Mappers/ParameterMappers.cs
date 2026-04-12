using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Entities;

namespace Shared.Domain.Mappers
{
   public static class ParameterMappers
   {
      public static ParameterDto ToParameterDto(this Parameter parameter)
      {
         return new ParameterDto
         {
            Id = parameter.Id,
            Module = parameter.Module,
            Group = parameter.Group,
            Name = parameter.Name,
            Key = parameter.Key,
            Title = parameter.Title,
            Description = parameter.Description,
            Type = parameter.Type,
            Value = parameter.Value,
            ListItems = parameter.ListItems,
            ExternalListEndpoint = parameter.ExternalListEndpoint,
            OverrideType = parameter.OverrideType,
            IsVisible = parameter.IsVisible
         };
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
}
