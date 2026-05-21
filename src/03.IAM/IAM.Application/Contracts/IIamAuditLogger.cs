using Shared.Domain.Enums;

namespace IAM.Application.Contracts;

public interface IIamAuditLogger
{
   Task LogAsync(
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      string description,
      Guid? targetId = null,
      object? metadata = null,
      CancellationToken cancellationToken = default);
}
