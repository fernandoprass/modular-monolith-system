using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using IAM.Infrastructure.Configurations;
using IAM.Domain;

namespace IAM.Infrastructure;

public class IamDbContext(DbContextOptions<IamDbContext> options) : DbContext(options)
{
   public DbSet<Customer> Customers { get; set; }
   public DbSet<Permission> Permissions { get; set; }
   public DbSet<Role> Roles { get; set; }
   public DbSet<RolePermission> RolePermissions { get; set; }
   public DbSet<User> Users { get; set; }
   public DbSet<UserRole> UserRoles { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
      modelBuilder.HasDefaultSchema(IamConst.Database.Schema);

      modelBuilder.ApplyConfiguration(new CustomerConfiguration());
      modelBuilder.ApplyConfiguration(new UserConfiguration());
      modelBuilder.ApplyConfiguration(new RoleConfiguration());
      modelBuilder.ApplyConfiguration(new PermissionConfiguration());
      modelBuilder.ApplyConfiguration(new RoleFeatureConfiguration());
      modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
   }

   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
   {
      configurationBuilder.Properties<Guid>().HaveColumnType(IamConst.Database.UuidType);
   }
}