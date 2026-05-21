using IAM.Application.Contracts;
using IAM.Domain;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using System.Text.Json;

namespace IAM.Application.Services;

public class IamAuditLogger(
   IEventPublisher eventPublisher,
   IUserContext userContext) : IIamAuditLogger
{
   private readonly IEventPublisher _eventPublisher = eventPublisher;
   private readonly IUserContext _userContext = userContext;

   public async Task LogAsync(
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      string description,
      Guid? targetId = null,
      object? metadata = null,
      CancellationToken cancellationToken = default)
   {
      var auditLog = new AuditLogEvent
      {
         Module = IamConst.System.ModuleName.ToLowerInvariant(),
         Feature = feature,
         Action = action,
         PrivacyLevel = privacyLevel,
         Description = description,
         UserId = _userContext.UserId,
         OrganizationId = _userContext.UserOwnerId,
         IpAddress = _userContext.IpAddress,
         UserAgent = _userContext.UserAgent,
         TargetId = targetId ?? Guid.Empty,
         Metadata = JsonSerializer.Serialize(metadata ?? new { })
      };

      await _eventPublisher.PublishAuditLogEventAsync(auditLog, cancellationToken);
   }
}
