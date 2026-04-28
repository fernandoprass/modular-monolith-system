using IAM.Domain.DTOs.Responses;

namespace IAM.Domain.QueryRepositories;

public interface IPermissionQueryRepository
{
   Task<PermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<PermissionDto>> GetAllAsync(string module, string group, string name, CancellationToken cancellationToken = default);
   Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
