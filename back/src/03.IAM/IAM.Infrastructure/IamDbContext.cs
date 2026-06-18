using IAM.Domain;
using IAM.Domain.Entities;
using IAM.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace IAM.Infrastructure;

public class IamDbContext(DbContextOptions<IamDbContext> options) : DbContext(options)
{
   public DbSet<Organization> Organizations { get; set; }
   public DbSet<Permission> Permissions { get; set; }
   public DbSet<Role> Roles { get; set; }
   public DbSet<RolePermission> RolePermissions { get; set; }
   public DbSet<User> Users { get; set; }
   public DbSet<UserRole> UserRoles { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
      modelBuilder.HasDefaultSchema(IamConst.Database.Schema);

      modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
      modelBuilder.ApplyConfiguration(new UserConfiguration());
      modelBuilder.ApplyConfiguration(new RoleConfiguration());
      modelBuilder.ApplyConfiguration(new PermissionConfiguration());
      modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
      modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
   }

   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
   {
      configurationBuilder.Properties<Guid>().HaveColumnType(SharedConst.Database.PostgreSQL.UuidType);
   }
}