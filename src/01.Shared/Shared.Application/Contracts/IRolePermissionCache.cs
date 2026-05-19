namespace Shared.Application.Contracts;

public interface IRolePermissionCache
{
   Task<IReadOnlyCollection<string>> GetPermissionsAsync(string role, CancellationToken cancellationToken = default);

   Task SetPermissionsAsync(string role, IEnumerable<string> permissions, DateTime expiresAt, CancellationToken cancellationToken = default);

   Task RemovePermissionsAsync(string role, CancellationToken cancellationToken = default);
}
