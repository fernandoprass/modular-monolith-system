using Sentinel.Domain.Entities;
using Shared.Domain.Interfaces;

namespace Sentinel.Domain.Interfaces;

public interface IAuditLogRepository : IBaseRepository<AuditLog>
{
   Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
