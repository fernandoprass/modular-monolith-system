using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;

namespace Shared.Infrastructure.Tests;

public class BaseRepositoryTests
{
   private readonly DbContextOptions<TestDbContext> _options;

   public BaseRepositoryTests() => _options = new DbContextOptionsBuilder<TestDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

   [Fact]
   public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntity()
   {
      using var context = new TestDbContext(_options);
      var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
      context.TestEntities.Add(entity);
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var repository = new BaseRepository<TestEntity>(context);

      var result = await repository.GetByIdAsync(entity.Id, TestContext.Current.CancellationToken);

      result.Should().NotBeNull();
      result!.Name.Should().Be("Test");
   }

   [Fact]
   public async Task GetByIdAsync_WhenEntityDoesNotExist_ShouldReturnNull()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);

      var result = await repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

      result.Should().BeNull();
   }

   [Fact]
   public async Task AddAsync_ShouldAddEntityToContext()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);
      var entity = new TestEntity { Id = Guid.NewGuid(), Name = "New Entity" };

      await repository.AddAsync(entity, TestContext.Current.CancellationToken);
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);

      context.TestEntities.Should().Contain(entity);
   }

   [Fact]
   public void Update_ShouldChangeEntityStateToModified()
   {
      using var context = new TestDbContext(_options);
      var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
      context.TestEntities.Add(entity);
      context.SaveChanges();

      var repository = new BaseRepository<TestEntity>(context);
      entity.Name = "Updated";

      repository.Update(entity);

      context.Entry(entity).State.Should().Be(EntityState.Modified);
   }

   [Fact]
   public void Update_WhenEntityIsDetached_ShouldAttachAsModified()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);
      var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Detached" };

      repository.Update(entity);

      context.Entry(entity).State.Should().Be(EntityState.Modified);
   }

   [Fact]
   public async Task DeleteAsync_WhenEntityExists_ShouldRemoveFromContext()
   {
      using var context = new TestDbContext(_options);
      var entity = new TestEntity { Id = Guid.NewGuid() };
      context.TestEntities.Add(entity);
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var repository = new BaseRepository<TestEntity>(context);

      await repository.DeleteAsync(entity.Id, TestContext.Current.CancellationToken);
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);

      context.TestEntities.Should().NotContain(entity);
   }

   [Fact]
   public async Task DeleteAsync_WhenEntityDoesNotExist_ShouldNotChangeContext()
   {
      using var context = new TestDbContext(_options);
      var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Keep" };
      context.TestEntities.Add(entity);
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var repository = new BaseRepository<TestEntity>(context);

      await repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);

      context.TestEntities.Should().ContainSingle();
      context.TestEntities.Single().Name.Should().Be("Keep");
   }

   [Fact]
   public async Task ExistsAsync_WhenIdExists_ShouldReturnTrue()
   {
      using var context = new TestDbContext(_options);
      var id = Guid.NewGuid();
      context.TestEntities.Add(new TestEntity { Id = id });
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var repository = new BaseRepository<TestEntity>(context);

      var exists = await repository.ExistsAsync(id, TestContext.Current.CancellationToken);

      exists.Should().BeTrue();
   }

   [Fact]
   public async Task ExistsAsync_WhenIdDoesNotExist_ShouldReturnFalse()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);

      var exists = await repository.ExistsAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

      exists.Should().BeFalse();
   }

   [Fact]
   public async Task GenericRepository_WhenEntityUsesStringId_ShouldFindEntity()
   {
      using var context = new TestDbContext(_options);
      var entity = new StringIdTestEntity { Id = "key-1", Name = "String Id" };
      context.StringIdTestEntities.Add(entity);
      await context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var repository = new BaseRepository<StringIdTestEntity, string>(context);

      var result = await repository.GetByIdAsync(entity.Id, TestContext.Current.CancellationToken);

      result.Should().NotBeNull();
      result!.Name.Should().Be("String Id");
   }

   [Fact]
   public async Task GetByIdAsync_WhenCancelled_ShouldThrowException()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);
      var cts = new CancellationTokenSource();
      await cts.CancelAsync();

      Func<Task> act = async () => await repository.GetByIdAsync(Guid.NewGuid(), cts.Token);

      await act.Should().ThrowAsync<OperationCanceledException>();
   }

   [Fact]
   public async Task AddAsync_WhenCancelled_ShouldThrowException()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);
      var entity = new TestEntity { Id = Guid.NewGuid(), Name = "To be cancelled" };
      var cts = new CancellationTokenSource();
      await cts.CancelAsync();

      Func<Task> act = async () => await repository.AddAsync(entity, cts.Token);

      await act.Should().ThrowAsync<OperationCanceledException>();
   }

   [Fact]
   public async Task DeleteAsync_WhenCancelled_ShouldThrowException()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);
      var cts = new CancellationTokenSource();
      await cts.CancelAsync();

      Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid(), cts.Token);

      await act.Should().ThrowAsync<OperationCanceledException>();
   }

   [Fact]
   public async Task ExistsAsync_WhenCancelled_ShouldThrowException()
   {
      using var context = new TestDbContext(_options);
      var repository = new BaseRepository<TestEntity>(context);
      var cts = new CancellationTokenSource();
      await cts.CancelAsync();

      Func<Task> act = async () => await repository.ExistsAsync(Guid.NewGuid(), cts.Token);

      await act.Should().ThrowAsync<OperationCanceledException>();
   }

   private class TestEntity : Entity
   {
      public string Name { get; set; } = string.Empty;
   }

   private class StringIdTestEntity : Entity<string>
   {
      public string Name { get; set; } = string.Empty;
   }

   private class TestDbContext : DbContext
   {
      public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
      public DbSet<TestEntity> TestEntities { get; set; }
      public DbSet<StringIdTestEntity> StringIdTestEntities { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
         modelBuilder.Entity<StringIdTestEntity>().HasKey(e => e.Id);
      }
   }
}
