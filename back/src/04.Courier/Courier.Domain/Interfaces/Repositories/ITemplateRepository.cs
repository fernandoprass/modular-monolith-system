using Courier.Domain.Entities;
using Shared.Domain.Enums;

namespace Courier.Domain.Interfaces.Repositories;

public interface ITemplateRepository
{
   Task<Template?> GetByModuleAndKeyAsync(string module, string key, CancellationToken cancellationToken = default);
   Task<RetentionPolicy?> GetRetentionPolicyByModuleAndKeyAsync(string module, string key, CancellationToken cancellationToken = default);
}
