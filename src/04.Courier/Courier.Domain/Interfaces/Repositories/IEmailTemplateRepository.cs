using Courier.Domain.Enums;
using Courier.Domain.Entities;

namespace Courier.Domain.Interfaces.Repositories;

public interface IEmailTemplateRepository
{
   Task<EmailTemplate?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
   Task<EmailRetentionPolicy?> GetRetentionPolicyByKeyAsync(string key, CancellationToken cancellationToken = default);
}
