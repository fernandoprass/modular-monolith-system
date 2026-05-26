using IAM.Application.Contracts;
using IAM.Domain;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using System.Text.Json;

namespace IAM.Application.Services;

public class IamEventPublisher(
   IEventPublisher eventPublisher,
   IUserContext userContext) : IIamEventPublisher
{
   private readonly IEventPublisher _eventPublisher = eventPublisher;
   private readonly IUserContext _userContext = userContext;

   public async Task NotifyAuditLogAsync(
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

   public async Task NotifyEmailAsync(
   string templateKey,
   Guid organizationId,
   Guid userId,
   string recipient,
   string feature,
   IReadOnlyDictionary<string, string>? values = null,
   CancellationToken cancellationToken = default)
   {
      var emailRequest = new EmailRequestedEvent(
         organizationId,
         userId,
         IamConst.System.ModuleName.ToLowerInvariant(),
         feature,
         templateKey,
         GetLanguage(),
         recipient,
         values);

      await _eventPublisher.PublishEmailRequestedEventAsync(emailRequest, cancellationToken);
   }

   private string GetLanguage()
   {
      return string.IsNullOrWhiteSpace(_userContext.Language)
         ? SharedConst.System.DefaultLanguage
         : _userContext.Language.Trim().ToLowerInvariant();
   }
}
