using Microsoft.EntityFrameworkCore;
using Sentinel.Domain;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Configurations;

namespace Sentinel.Infrastructure;

public class SentinelDbContext(DbContextOptions<SentinelDbContext> options) : DbContext(options)
{
   public DbSet<AuditLog> AuditLogs { get; set; }
   public DbSet<SystemLog> SystemLogs { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
      modelBuilder.HasDefaultSchema(SentinelConst.Database.Schema);

      modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
      modelBuilder.ApplyConfiguration(new SystemLogConfiguration());
   }

   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
   {
      configurationBuilder.Properties<Guid>().HaveColumnType(SentinelConst.Database.UuidType);
   }
}
