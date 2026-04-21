using Shared.Domain.Entities;

namespace Shared.Domain.Interfaces;

/// <summary>
/// Interface for a generic repository pattern, providing basic CRUD operations for entities.
/// </summary>
/// <typeparam name="T">The Type of the Entity</typeparam>
/// <typeparam name="TId">The Type of the Entity Id</typeparam>
public interface IBaseRepository<T, TId> where T : Entity<TId>
{
   Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
   Task AddAsync(T entity, CancellationToken cancellationToken = default);
   void Update(T entity); // Synchronous by design
   Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
   Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// This interface is a specialization of the IBaseRepository for entities that use Guid as their identifier type. 
/// It inherits all the CRUD operations defined in the generic interface, but it assumes that the identifier for 
/// the entity is of type Guid, which is a common choice for unique identifiers in many applications.
/// </summary>
/// <typeparam name="T">The Type of the Entity</typeparam>
public interface IBaseRepository<T> : IBaseRepository<T, Guid> where T : Entity
{
}
