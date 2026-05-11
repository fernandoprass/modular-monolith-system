using Sentinel.Domain.Entities;
using Shared.Domain.Interfaces;

namespace Sentinel.Domain.Interfaces;

public interface ISystemLogRepository : IBaseRepository<SystemLog>
{
   Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
