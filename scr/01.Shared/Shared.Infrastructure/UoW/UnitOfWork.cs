using Shared.Domain.Entities;
using Shared.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;

namespace Shared.Infrastructure.UoW;

/// <summary>
/// UnitOfWork is an abstract class that implements the IUnitOfWork interface, providing a base 
/// implementation for managing database transactions and ensuring that changes to entities are 
/// tracked and saved correctly. It uses Entity Framework Core's DbContext to manage the database 
/// context and includes logic to automatically set audit fields (CreatedAt, CreatedBy, UpdatedAt, 
/// UpdatedBy) based on the state of the entities being tracked. The class also ensures that the 
/// database context is properly disposed of when the unit of work is no longer needed.
/// </summary>
/// <typeparam name="TContext">The database context</typeparam>
public class UnitOfWork<TContext>(TContext dbContext, IUserContext userContext) 
   : IUnitOfWork<TContext> where TContext : DbContext
{
    private readonly TContext _dbContext = dbContext;
    private readonly IUserContext _userContext = userContext;

   public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        var entries = _dbContext.ChangeTracker.Entries<EntityAudited>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;

                // For the case when creating the first user (or system user), 
                // the CreatedBy should be the user itself if no context user exists.
                // We assume there's a convention or specific check for "User" type if needed, 
                // but usually, it's safer to just check if userId is empty.
                entry.Entity.CreatedBy = userId == Guid.Empty ? entry.Entity.CreatedBy : userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = _userContext.UserId == Guid.Empty ? null : _userContext.UserId;
            }
        }
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
