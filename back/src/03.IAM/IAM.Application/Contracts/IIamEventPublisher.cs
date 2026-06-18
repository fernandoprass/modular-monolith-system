using Shared.Domain.Enums;

namespace IAM.Application.Contracts;

public interface IIamEventPublisher
{
   Task NotifyAuditLogAsync(
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      RetentionPolicy retentionPolicy,
      string description,
      Guid? targetId = null,
      object? metadata = null,
      CancellationToken cancellationToken = default);

   Task NotifyEmailAsync(
      string templateKey,
      Guid organizationId,
      Guid userId,
      string recipient,
      string feature,
      IReadOnlyDictionary<string, string>? values = null,
      CancellationToken cancellationToken = default);
}
