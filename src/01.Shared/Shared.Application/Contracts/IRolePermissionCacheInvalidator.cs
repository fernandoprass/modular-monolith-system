namespace Shared.Application.Contracts;

public interface IRolePermissionCacheInvalidator
{
   Task RemoveAsync(Guid roleId, CancellationToken cancellationToken = default);
}
