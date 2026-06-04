using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.Entities;

namespace Sentinel.Domain.Mappers;

public static class AuditLogMappers
{
   public static AuditLogDto ToAuditLogDto(this AuditLog auditLog)
   {
      return new AuditLogDto(
         auditLog.Id,
         auditLog.Module,
         auditLog.Feature,
         auditLog.Action,
         auditLog.PrivacyLevel,
         auditLog.Description,
         auditLog.CreatedAt,
         auditLog.ExpiresAt,
         auditLog.UserId,
         auditLog.OrganizationId,
         auditLog.TargetId,
         auditLog.IpAddress,
         auditLog.UserAgent,
         auditLog.Metadata);
   }
}
