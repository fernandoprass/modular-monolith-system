using Shared.Application.Contracts;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Repositories;

namespace Shared.Infrastructure.UoW;

/// <summary>
/// SharedUnitOfWork is a concrete implementation of the ISharedUnitOfWork interface, which extends the generic UnitOfWork class.
/// </summary>
internal class SharedUnitOfWork(SharedDbContext dbContext, IUserContext userContext)
   : UnitOfWork<SharedDbContext>(dbContext, userContext), ISharedUnitOfWork
{
   private readonly SharedDbContext _dbContext = dbContext;

   public IParameterRepository Parameters => new ParameterRepository(_dbContext);
   public IParameterOverrideRepository ParameterOverrides => new ParameterOverrideRepository(_dbContext);
}