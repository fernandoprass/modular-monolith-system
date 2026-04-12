using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Repositories;

/// <summary>
/// The BaseRepository class provides a generic implementation of the IRepository interface for basic CRUD operations on entities. 
/// It uses Entity Framework Core's DbContext and DbSet to interact with the database. This class can be inherited by specific 
/// repositories for different entities, allowing for code reuse and consistency across the data access layer.
/// </summary>
/// <typeparam name="T">The Type of the Entity</typeparam>
/// <typeparam name="TId">The Type of the Entity Id</typeparam>
/// <param name="context">The DbContext instance used for database operations</param>
public class BaseRepository<T, TId>(DbContext context) : IBaseRepository<T, TId>
    where T : Entity<TId>
{
   protected readonly DbContext _context = context;
   protected readonly DbSet<T> _dbSet = context.Set<T>();

   public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
   {
      return await _dbSet.FindAsync([id], cancellationToken);
   }

   public virtual Task AddAsync(T entity, CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();
      return _dbSet.AddAsync(entity, cancellationToken).AsTask();
   }

   public virtual void Update(T entity)
   {
      _dbSet.Update(entity);
   }

   public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
   {
      var entity = await _dbSet.FindAsync([id], cancellationToken);
      if (entity != null)
      {
         _dbSet.Remove(entity);
      }
   }

   public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
   {
      return await _dbSet.AnyAsync(e => e.Id.Equals(id), cancellationToken);
   }
}

/// <summary>
/// The BaseRepository class is a specialization of the generic BaseRepository for entities that use Guid as their identifier type.
/// </summary>
/// <typeparam name="T">The Type of the Entity</typeparam>
/// <param name="context">The DbContext instance used for database operations</param>
public class BaseRepository<T>(DbContext context) 
   : BaseRepository<T, Guid>(context) where T : Entity<Guid>
{

}