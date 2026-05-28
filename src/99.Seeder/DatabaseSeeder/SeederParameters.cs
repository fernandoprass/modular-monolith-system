using DatabaseSeeder.Interfaces;
using DatabaseSeeder.Parameters;
using Shared.Infrastructure;

namespace DatabaseSeeder;

public class SeederParameters(ISeederData seederData, SharedDbContext dbContext)
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      Console.WriteLine("Starting to add parameters...");

      await new SeederParametersIam(seederData, dbContext).SeedAsync(cancellationToken);
      await new SeederParametersCourier(dbContext).SeedAsync(cancellationToken);

      await dbContext.SaveChangesAsync(cancellationToken);

      Console.WriteLine("Finished adding parameters...");
      Console.WriteLine();
   }
}
