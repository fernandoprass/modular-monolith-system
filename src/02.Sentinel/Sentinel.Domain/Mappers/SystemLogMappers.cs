using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.Entities;

namespace Sentinel.Domain.Mappers;

public static class SystemLogMappers
{
   public static SystemLogDto ToSystemLogDto(this SystemLog systemLog)
   {
      return new SystemLogDto(
         systemLog.Id,
         systemLog.CreatedAt,
         systemLog.Level,
         systemLog.Status,
         systemLog.Source,
         systemLog.Message,
         systemLog.Exception,
         systemLog.StackTrace,
         systemLog.RequestId,
         systemLog.UserId,
         systemLog.OrganizationId,
         systemLog.PropertiesJson);
   }
}
