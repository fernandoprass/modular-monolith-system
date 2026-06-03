using Courier.Domain.Entities;
using Shared.Domain.Enums;

namespace Courier.Domain.Interfaces.Repositories;

public interface ITemplateRepository
{
   Task<Template?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
   Task<RetentionPolicy?> GetRetentionPolicyByKeyAsync(string key, CancellationToken cancellationToken = default);
}
