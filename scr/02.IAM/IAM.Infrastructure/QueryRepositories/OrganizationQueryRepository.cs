using IAM.Domain.DTOs.Responses;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.QueryRepositories;

public class OrganizationQueryRepository(IamDbContext context) : IOrganizationQueryRepository
{
   private readonly IamDbContext _context = context;

   public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _context.Organizations
          .AsNoTracking()
          .Where(c => c.Id == id)
          .Select(c => new OrganizationDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<IEnumerable<OrganizationDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
   {
      return await _context.Organizations
          .AsNoTracking()
          .Where(c => c.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
          .Select(c => new OrganizationDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<OrganizationDto>> GetAllAsync(CancellationToken cancellationToken = default)
   {
      return await _context.Organizations
          .AsNoTracking()
          .Select(c => new OrganizationDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .ToListAsync(cancellationToken);
   }

   public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _context.Organizations.AnyAsync(c => c.Code == code, cancellationToken);
   }
}