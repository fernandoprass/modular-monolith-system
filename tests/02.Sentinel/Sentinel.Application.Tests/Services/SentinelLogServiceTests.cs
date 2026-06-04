using NSubstitute;
using Sentinel.Application.Services;
using Sentinel.Domain.DTOs.Requests;
using Sentinel.Domain.DTOs.Responses;
using Sentinel.Domain.QueryRepositories;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Sentinel.Application.Tests.Services;

public class SentinelLogServiceTests
{
   private readonly ISentinelLogQueryRepository _queryRepository = Substitute.For<ISentinelLogQueryRepository>();
   private readonly IUserContext _userContext = Substitute.For<IUserContext>();
   private readonly SentinelLogService _service;

   public SentinelLogServiceTests()
   {
      _service = new SentinelLogService(_queryRepository, _userContext);
   }

   [Fact]
   public async Task GetAuditLogByIdAsync_ShouldReturnAuditLog_WhenFound()
   {
      var id = Guid.NewGuid();
      var auditLog = new AuditLogDto(
         id,
         "iam",
         "users",
         "create",
         AuditPrivacyLevel.Medium,
         "Created user",
         DateTime.UtcNow,
         DateTime.UtcNow.AddDays(90),
         Guid.NewGuid(),
         Guid.NewGuid(),
         Guid.NewGuid(),
         "127.0.0.1",
         "test-agent",
         "{}");

      _queryRepository.GetAuditLogByIdAsync(id, _userContext, Arg.Any<CancellationToken>()).Returns(auditLog);

      var result = await _service.GetAuditLogByIdAsync(id, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal(auditLog, result.Data);
   }

   [Fact]
   public async Task GetAuditLogByIdAsync_ShouldReturnNotFound_WhenMissing()
   {
      var id = Guid.NewGuid();
      _queryRepository.GetAuditLogByIdAsync(id, _userContext, Arg.Any<CancellationToken>()).Returns((AuditLogDto?)null);

      var result = await _service.GetAuditLogByIdAsync(id, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, message => message is NotFoundError);
   }

   [Fact]
   public async Task GetSystemLogByIdAsync_ShouldReturnSystemLog_WhenFound()
   {
      var id = Guid.NewGuid();
      var systemLog = new SystemLogDto(
         id,
         SystemLogLevel.Error,
         SystemLogStatus.Failure,
         "Sentinel",
         "Failure",
         "Exception",
         "Stack",
         DateTime.UtcNow,
         DateTime.UtcNow.AddDays(90),
         "request-1",
         Guid.NewGuid(),
         Guid.NewGuid(),
         "{}");

      _queryRepository.GetSystemLogByIdAsync(id, _userContext, Arg.Any<CancellationToken>()).Returns(systemLog);

      var result = await _service.GetSystemLogByIdAsync(id, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal(systemLog, result.Data);
   }

   [Fact]
   public async Task GetSystemLogByIdAsync_ShouldReturnNotFound_WhenMissing()
   {
      var id = Guid.NewGuid();
      _queryRepository.GetSystemLogByIdAsync(id, _userContext, Arg.Any<CancellationToken>()).Returns((SystemLogDto?)null);

      var result = await _service.GetSystemLogByIdAsync(id, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Messages, message => message is NotFoundError);
   }

   [Fact]
   public async Task GetAuditLogsByParamsAsync_ShouldReturnRepositoryPage()
   {
      var request = new AuditLogSearchRequest(null, null, "iam", null, null, null, null, null, null);
      var page = new PagedResultDto<AuditLogLiteDto>([], 1, 50, 0, 0);
      _queryRepository.GetAuditLogsByParamsAsync(request, _userContext, Arg.Any<CancellationToken>()).Returns(page);

      var result = await _service.GetAuditLogsByParamsAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal(page, result.Data);
      await _queryRepository.Received(1).GetAuditLogsByParamsAsync(request, _userContext, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetSystemLogsByParamsAsync_ShouldReturnRepositoryPage()
   {
      var request = new SystemLogSearchRequest(null, null, SystemLogLevel.Error, null, null, null, null, null);
      var page = new PagedResultDto<SystemLogLiteDto>([], 1, 50, 0, 0);
      _queryRepository.GetSystemLogsByParamsAsync(request, _userContext, Arg.Any<CancellationToken>()).Returns(page);

      var result = await _service.GetSystemLogsByParamsAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal(page, result.Data);
      await _queryRepository.Received(1).GetSystemLogsByParamsAsync(request, _userContext, Arg.Any<CancellationToken>());
   }
}
