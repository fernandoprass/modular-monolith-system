using Microsoft.EntityFrameworkCore;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.QueryRepositories;
using Shared.Application.Contracts;
using Shared.Domain.Enums;

namespace Sentinel.Infrastructure.Tests;

public class SentinelLogQueryRepositoryTests
{
   [Fact]
   public async Task GetAuditLogsByParamsAsync_WhenNotSystemAdmin_FiltersByUserOwnerId()
   {
      var organizationId = Guid.CreateVersion7();
      var otherOrganizationId = Guid.CreateVersion7();
      await using var context = CreateContext();

      await context.AuditLogs.AddAsync(CreateAuditLog(organizationId));
      await context.AuditLogs.AddAsync(CreateAuditLog(otherOrganizationId));
      await context.SaveChangesAsync();

      var repository = new SentinelLogQueryRepository(context);
      var userContext = new TestUserContext(organizationId, isSystemAdmin: false);

      var result = await repository.GetAuditLogsByParamsAsync(new AuditLogSearchRequest(otherOrganizationId, null, null, null, null, null, null, null, null), userContext);

      Assert.Single(result.Items);
      Assert.Equal(1, result.TotalCount);
      Assert.All(result.Items, auditLog => Assert.Equal(organizationId, auditLog.OrganizationId));
   }

   [Fact]
   public async Task GetSystemLogsByParamsAsync_WhenSystemAdmin_UsesRequestedOrganizationId()
   {
      var organizationId = Guid.CreateVersion7();
      var otherOrganizationId = Guid.CreateVersion7();
      await using var context = CreateContext();

      await context.SystemLogs.AddAsync(CreateSystemLog(organizationId));
      await context.SystemLogs.AddAsync(CreateSystemLog(otherOrganizationId));
      await context.SaveChangesAsync();

      var repository = new SentinelLogQueryRepository(context);
      var userContext = new TestUserContext(Guid.Empty, isSystemAdmin: true);

      var result = await repository.GetSystemLogsByParamsAsync(new SystemLogSearchRequest(organizationId, null, null, null, null, null, null, null), userContext);

      Assert.Single(result.Items);
      Assert.Equal(1, result.TotalCount);
      Assert.All(result.Items, systemLog => Assert.Equal(organizationId, systemLog.OrganizationId));
   }

   [Fact]
   public async Task GetAuditLogsByParamsAsync_WhenPaginationRequested_ReturnsRequestedPage()
   {
      var organizationId = Guid.CreateVersion7();
      await using var context = CreateContext();

      await context.AuditLogs.AddAsync(CreateAuditLog(organizationId));
      await context.AuditLogs.AddAsync(CreateAuditLog(organizationId));
      await context.AuditLogs.AddAsync(CreateAuditLog(organizationId));
      await context.SaveChangesAsync();

      var repository = new SentinelLogQueryRepository(context);
      var userContext = new TestUserContext(organizationId, isSystemAdmin: false);

      var result = await repository.GetAuditLogsByParamsAsync(new AuditLogSearchRequest(null, null, null, null, null, null, null, null, null, 2, 2), userContext);

      Assert.Single(result.Items);
      Assert.Equal(2, result.PageNumber);
      Assert.Equal(2, result.PageSize);
      Assert.Equal(3, result.TotalCount);
      Assert.Equal(2, result.TotalPages);
   }

   [Fact]
   public async Task GetSystemLogsByParamsAsync_WhenPageSizeTooLarge_ClampsToMaxPageSize()
   {
      var organizationId = Guid.CreateVersion7();
      await using var context = CreateContext();

      await context.SystemLogs.AddAsync(CreateSystemLog(organizationId));
      await context.SaveChangesAsync();

      var repository = new SentinelLogQueryRepository(context);
      var userContext = new TestUserContext(organizationId, isSystemAdmin: false);

      var result = await repository.GetSystemLogsByParamsAsync(new SystemLogSearchRequest(null, null, null, null, null, null, null, null, 1, 500), userContext);

      Assert.Single(result.Items);
      Assert.Equal(200, result.PageSize);
      Assert.Equal(1, result.TotalCount);
   }

   private static SentinelDbContext CreateContext()
   {
      var options = new DbContextOptionsBuilder<SentinelDbContext>()
         .UseInMemoryDatabase(Guid.CreateVersion7().ToString())
         .Options;

      return new SentinelDbContext(options);
   }

   private static AuditLog CreateAuditLog(Guid organizationId)
   {
      return AuditLog.Create(
         DateTime.UtcNow,
         "IAM",
         "users",
         "created",
         AuditPrivacyLevel.Medium,
         "Created user",
         Guid.CreateVersion7(),
         organizationId,
         Guid.CreateVersion7(),
         null,
         null,
         "{}");
   }

   private static SystemLog CreateSystemLog(Guid organizationId)
   {
      return SystemLog.Create(
         DateTime.UtcNow,
         SystemLogLevel.Error,
         SystemLogStatus.Failure,
         "IAM.API",
         "Failed",
         null,
         null,
         null,
         Guid.CreateVersion7(),
         organizationId,
         "{}");
   }

   private sealed class TestUserContext(Guid userOwnerId, bool isSystemAdmin) : IUserContext
   {
      public Guid UserOwnerId { get; } = userOwnerId;
      public bool IsAuthenticated => true;
      public bool IsSystemAdmin { get; } = isSystemAdmin;
      public Guid UserId { get; } = Guid.CreateVersion7();
      public IEnumerable<string> Roles => [];
   }
}
