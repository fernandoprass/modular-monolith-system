using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using IAM.Infrastructure.Repositories;
using Shared.Application.Contracts;
using Shared.Infrastructure.UoW;

namespace IAM.Infrastructure.UoW;

/// <summary>
/// SharedUnitOfWork is a concrete implementation of the ISharedUnitOfWork interface, which extends the generic UnitOfWork class.
/// </summary>
public class IamUnitOfWork : UnitOfWork<IamDbContext>, IIamUnitOfWork
{
   private readonly IamDbContext _dbContext;

   public IamUnitOfWork(IamDbContext dbContext, IUserContext userContext)
       : base(dbContext, userContext)
   {
      _dbContext = dbContext;
   }

   public IOrganizationRepository Organizations => new OrganizationRepository(_dbContext);
   public IRoleRepository Roles => new RoleRepository(_dbContext);
   public IUserRepository Users => new UserRepository(_dbContext);
}