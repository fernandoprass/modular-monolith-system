using Courier.Domain.Enums;

namespace Courier.Domain.Interfaces.Repositories;

public interface IEmailTemplateRepository
{
   Task<EmailRetentionPolicy?> GetRetentionPolicyByKeyAsync(string key, CancellationToken cancellationToken = default);
}
