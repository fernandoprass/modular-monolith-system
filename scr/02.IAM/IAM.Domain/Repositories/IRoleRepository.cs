using IAM.Domain.Entities;

namespace IAM.Domain.Repositories
{
   public interface IRoleRepository
   {
      Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
      Task AddAsync(Role role, CancellationToken cancellationToken = default);
      void Update(Role role);
      Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   }
}
