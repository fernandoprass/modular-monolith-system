using IAM.Domain.Entities;
using Shared.Domain.Interfaces;

namespace IAM.Domain.Repositories;

public interface IPermissionRepository : IBaseRepository<Permission, Guid>
{
   Task<int> CountByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
   Task<IEnumerable<Permission>> GetAll(CancellationToken cancellationToken = default);
}
