using IAM.Domain.Repositories;
using Shared.Domain.Interfaces;

namespace IAM.Domain.Interfaces;

public interface IIamUnitOfWork : IUnitOfWork
{
   IOrganizationRepository Organizations { get; }
   IRoleRepository Roles { get; }
   IUserRepository Users { get; }
   IPermissionRepository Permissions { get; }
}