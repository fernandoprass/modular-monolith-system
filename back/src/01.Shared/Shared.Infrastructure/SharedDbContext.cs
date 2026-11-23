using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Infrastructure.Configurations;

namespace Shared.Infrastructure;

public class SharedDbContext(DbContextOptions<SharedDbContext> options) : DbContext(options)
{
   internal DbSet<Lookup> Lookups { get; set; }
   internal DbSet<LookupTranslation> LookupTranslations { get; set; }
   internal DbSet<LookupType> LookupTypes { get; set; }
   public DbSet<Parameter> Parameters { get; set; }
   public DbSet<ParameterOverride> ParameterOverrides { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
      modelBuilder.HasDefaultSchema(Domain.SharedConst.Database.Schema);

      modelBuilder.ApplyConfiguration<Parameter>(new ParameterConfiguration());
      modelBuilder.ApplyConfiguration<ParameterOverride>(new ParameterOverrideConfiguration());
      modelBuilder.ApplyConfiguration(new LookupConfiguration());
      modelBuilder.ApplyConfiguration(new LookupTranslationConfiguration());
      modelBuilder.ApplyConfiguration(new LookupTypeConfiguration());
   }

   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
   {
      configurationBuilder.Properties<Guid>().HaveColumnType(SharedConst.Database.PostgreSQL.UuidType);
   }
}
