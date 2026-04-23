using IAM.Domain.Entities;
using IAM.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;

namespace IAM.Infrastructure.Repositories;

public class OrganizationRepository(IamDbContext dbContext) : BaseRepository<Organization>(dbContext), IOrganizationRepository
{
   public async Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
   {
      return await dbContext.Organizations.SingleOrDefaultAsync(c => c.Code == code, cancellationToken);
   }
}