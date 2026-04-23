using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.Entities;
using Shared.Infrastructure.UoW;

namespace Shared.Infrastructure.Tests;

public class UnitOfWorkTests
{
   private readonly IUserContext _userContextMock;
   private readonly DbContextOptions<TestDbContext> _options;

   public UnitOfWorkTests()
   {
      _userContextMock = Substitute.For<IUserContext>();
      _options = new DbContextOptionsBuilder<TestDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;
   }

   [Fact]
   public async Task SaveChangesAsync_ShouldPopulateAuditFields_WhenAdded()
   {
      var userId = Guid.NewGuid();
      _userContextMock.UserId.Returns(userId);
      using var context = new TestDbContext(_options);
      var uow = new UnitOfWork<TestDbContext>(context, _userContextMock);
      var entity = new TestAuditedEntity { Name = "New" };
      context.TestEntities.Add(entity);

      await uow.SaveChangesAsync(TestContext.Current.CancellationToken);

      entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
      entity.CreatedBy.Should().Be(userId);
   }

   [Fact]
   public async Task SaveChangesAsync_ShouldUseEntityCreatedBy_WhenUserContextIsEmpty()
   {
      _userContextMock.UserId.Returns(Guid.Empty);
      var systemUserId = Guid.NewGuid();
      using var context = new TestDbContext(_options);
      var uow = new UnitOfWork<TestDbContext>(context, _userContextMock);
      var entity = new TestAuditedEntity { Name = "First User", CreatedBy = systemUserId };
      context.TestEntities.Add(entity);

      await uow.SaveChangesAsync(TestContext.Current.CancellationToken);

      entity.CreatedBy.Should().Be(systemUserId);
   }

   [Fact]
   public async Task SaveChangesAsync_ShouldPopulateAuditFields_WhenModified()
   {
      var userId = Guid.NewGuid();
      _userContextMock.UserId.Returns(userId);
      using var context = new TestDbContext(_options);
      var entity = new TestAuditedEntity { Name = "Original" };
      context.TestEntities.Add(entity);
      await context.SaveChangesAsync();

      var uow = new UnitOfWork<TestDbContext>(context, _userContextMock);
      entity.Name = "Updated";
      context.Entry(entity).State = EntityState.Modified;

      await uow.SaveChangesAsync(TestContext.Current.CancellationToken);

      entity.UpdatedAt.Should().NotBeNull();
      entity.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
      entity.UpdatedBy.Should().Be(userId);
   }

   [Fact]
   public async Task SaveChangesAsync_ShouldPropagateCancellationToken()
   {
      using var context = new TestDbContext(_options);
      var uow = new UnitOfWork<TestDbContext>(context, _userContextMock);

      // Add something to ensure the context has changes
      context.TestEntities.Add(new TestAuditedEntity { Name = "To be cancelled" });

      var cts = new CancellationTokenSource();
      await cts.CancelAsync();

      Func<Task> act = async () => await uow.SaveChangesAsync(cts.Token);

      await act.Should().ThrowAsync<OperationCanceledException>();
   }

   [Fact]
   public void Dispose_ShouldDisposeDbContext()
   {
      var context = new TestDbContext(_options);
      var uow = new UnitOfWork<TestDbContext>(context, _userContextMock);

      uow.Dispose();

      Assert.Throws<ObjectDisposedException>(() => context.Set<TestAuditedEntity>().Add(new TestAuditedEntity()));
   }

   private class TestAuditedEntity : EntityAudited
   {
      public string Name { get; set; } = string.Empty;
   }

   private class TestDbContext : DbContext
   {
      public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
      public DbSet<TestAuditedEntity> TestEntities { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<TestAuditedEntity>().HasKey(e => e.Id);
      }
   }
}
