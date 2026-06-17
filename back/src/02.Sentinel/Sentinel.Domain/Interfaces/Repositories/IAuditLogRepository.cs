using Sentinel.Domain.Entities;

namespace Sentinel.Domain.Interfaces;

public interface IAuditLogRepository
{
   Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
   Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
