namespace IAM.Application.Contracts;

public interface IRolePermissionAuthorizationCache
{
   Task<IEnumerable<string>> GetOrCreateAsync(Guid roleId, Func<Task<IEnumerable<string>>> factory);
   void Remove(Guid roleId);
}
