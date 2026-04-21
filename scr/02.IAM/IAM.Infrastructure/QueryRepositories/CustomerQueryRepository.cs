using IAM.Domain.DTOs.Responses;
using IAM.Domain.QueryRepositories;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure.QueryRepositories;

public class CustomerQueryRepository(IamDbContext context) : ICustomerQueryRepository
{
   private readonly IamDbContext _context = context;

   public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _context.Customers
          .AsNoTracking()
          .Where(c => c.Id == id)
          .Select(c => new CustomerDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<IEnumerable<CustomerDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
   {
      return await _context.Customers
          .AsNoTracking()
          .Where(c => c.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
          .Select(c => new CustomerDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .ToListAsync(cancellationToken);
   }

   public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
   {
      return await _context.Customers
          .AsNoTracking()
          .Select(c => new CustomerDto(c.Id, c.Type, c.Code, c.Name, c.Description, c.IsActive))
          .ToListAsync(cancellationToken);
   }

   public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
   {
      return await _context.Customers.AnyAsync(c => c.Code == code, cancellationToken);
   }
}