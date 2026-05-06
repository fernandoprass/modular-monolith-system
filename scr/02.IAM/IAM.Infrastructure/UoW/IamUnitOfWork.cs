using IAM.Domain.Interfaces;
using IAM.Domain.Repositories;
using IAM.Infrastructure.Repositories;
using Shared.Application.Contracts;
using Shared.Infrastructure.UoW;

namespace IAM.Infrastructure.UoW;

/// <summary>
/// IamUnitOfWork is a concrete implementation of the IIamUnitOfWork interface, which extends the generic UnitOfWork class.
/// </summary>
public class IamUnitOfWork(IamDbContext dbContext, IUserContext userContext) : UnitOfWork<IamDbContext>(dbContext, userContext), IIamUnitOfWork
{
   private readonly IamDbContext _dbContext = dbContext;

   public IOrganizationRepository Organizations => new OrganizationRepository(_dbContext);
   public IRoleRepository Roles => new RoleRepository(_dbContext);
   public IUserRepository Users => new UserRepository(_dbContext);
   public IPermissionRepository Permissions => new PermissionRepository(_dbContext);
}