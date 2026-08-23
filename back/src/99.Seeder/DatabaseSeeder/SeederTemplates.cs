using Courier.Domain.Interfaces.Repositories;
using Courier.Infrastructure;

namespace DatabaseSeeder;

public class SeederTemplates(
   CourierDbContext courierDbContext,
   ITemplateRepository templateRepository,
   ITemplateWriteRepository templateWriteRepository)
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      await courierDbContext.ConfigureIndexesAsync(cancellationToken);
      await new Templates.Templates(templateRepository, templateWriteRepository).SeedAsync(cancellationToken);
   }
}
