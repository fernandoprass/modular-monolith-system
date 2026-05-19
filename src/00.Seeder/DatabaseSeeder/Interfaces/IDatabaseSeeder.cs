namespace DatabaseSeeder.Interfaces;

public interface IDatabaseSeeder
{
   Task SeedAsync(CancellationToken cancellationToken = default);
}
