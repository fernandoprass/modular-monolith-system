using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.Entities;

namespace Sentinel.Domain.Mappers;

public static class SystemLogMappers
{
   public static SystemLogDto ToSystemLogDto(this SystemLog systemLog)
   {
      return new SystemLogDto(
         systemLog.Id,
         systemLog.Level,
         systemLog.Status,
         systemLog.Module,
         systemLog.Message,
         systemLog.Exception,
         systemLog.StackTrace,
         systemLog.CreatedAt,
         systemLog.ExpiresAt,
         systemLog.RequestId,
         systemLog.UserId,
         systemLog.OrganizationId,
         systemLog.PropertiesJson);
   }
}
