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

   private class TestDbContext : DbContext
   {
      public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
      public DbSet<TestEntity> TestEntities { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
      }
   }
}