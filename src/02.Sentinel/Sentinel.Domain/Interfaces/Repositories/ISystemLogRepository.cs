using Sentinel.Domain.Entities;

namespace Sentinel.Domain.Interfaces;

public interface ISystemLogRepository
{
   Task AddAsync(SystemLog log, CancellationToken cancellationToken = default);
   Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
