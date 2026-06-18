using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Sentinel.API.Middlewares;
using Sentinel.Domain;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Interfaces;
using Shared.Application.Contracts;
using Shared.Domain.Enums;
using System.Text.Json;

namespace Sentinel.API.Tests.Middlewares;

public class GlobalExceptionHandlerTests
{
   [Theory]
   [InlineData("unauthorized", StatusCodes.Status401Unauthorized, "Unauthorized access.", SystemLogStatus.Unauthorized, 180)]
   [InlineData("generic", StatusCodes.Status500InternalServerError, "An unexpected error occurred.", SystemLogStatus.Failure, 30)]
   public async Task TryHandleAsync_ShouldReturnErrorResponseAndPersistSystemLog(
      string exceptionType,
      int expectedStatusCode,
      string expectedMessage,
      SystemLogStatus expectedLogStatus,
      int expectedRetentionDays)
   {
      var repository = new FakeSystemLogRepository();
      var unitOfWork = new FakeSentinelUnitOfWork(repository);
      var userId = Guid.CreateVersion7();
      var organizationId = Guid.CreateVersion7();
      var userContext = new FakeUserContext(userId, organizationId);
      var serviceProvider = CreateServiceProvider(unitOfWork, userContext);
      var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, serviceProvider);
      var httpContext = new DefaultHttpContext();
      httpContext.TraceIdentifier = "request-1";
      httpContext.Request.Path = "/api/v1/sentinel/logs";
      httpContext.Response.Body = new MemoryStream();
      var exception = CreateException(exceptionType);

      var handled = await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

      Assert.True(handled);
      Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);

      httpContext.Response.Body.Position = 0;
      using var response = await JsonDocument.ParseAsync(httpContext.Response.Body, cancellationToken: TestContext.Current.CancellationToken);
      Assert.Equal(expectedMessage, response.RootElement.GetProperty("message").GetString());

      var systemLog = Assert.Single(repository.Logs);
      Assert.Equal(SystemLogLevel.Error, systemLog.Level);
      Assert.Equal(expectedLogStatus, systemLog.Status);
      Assert.Equal(SentinelConst.System.ModuleName, systemLog.Module);
      Assert.Equal(exception.Message, systemLog.Message);
      Assert.Equal(exception.GetType().Name, systemLog.Exception);
      Assert.Equal("request-1", systemLog.RequestId);
      Assert.Equal(userId, systemLog.UserId);
      Assert.Equal(organizationId, systemLog.OrganizationId);
      Assert.True(systemLog.ExpiresAt >= systemLog.CreatedAt.AddDays(expectedRetentionDays).AddSeconds(-1));
      Assert.True(systemLog.ExpiresAt <= systemLog.CreatedAt.AddDays(expectedRetentionDays).AddSeconds(1));
      Assert.Contains("statusCode", systemLog.PropertiesJson);
      Assert.Contains("method", systemLog.PropertiesJson);
      Assert.Contains("path", systemLog.PropertiesJson);
      Assert.Equal(1, unitOfWork.SaveChangesCount);
   }

   [Fact]
   public async Task TryHandleAsync_ShouldStillReturnResponse_WhenSystemLogPersistenceFails()
   {
      var repository = new FakeSystemLogRepository { ShouldThrow = true };
      var unitOfWork = new FakeSentinelUnitOfWork(repository);
      var serviceProvider = CreateServiceProvider(unitOfWork, new FakeUserContext(Guid.Empty, Guid.Empty));
      var handler = new GlobalExceptionHandler(
         NullLogger<GlobalExceptionHandler>.Instance,
         serviceProvider);
      var httpContext = new DefaultHttpContext();
      httpContext.Response.Body = new MemoryStream();

      var handled = await handler.TryHandleAsync(httpContext, new InvalidOperationException("Boom"), TestContext.Current.CancellationToken);

      Assert.True(handled);
      Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
      Assert.Equal(0, unitOfWork.SaveChangesCount);
   }

   private static Exception CreateException(string exceptionType)
   {
      return exceptionType switch
      {
         "unauthorized" => new UnauthorizedAccessException("Denied"),
         _ => new InvalidOperationException("Boom")
      };
   }

   private static ServiceProvider CreateServiceProvider(ISentinelUnitOfWork unitOfWork, IUserContext userContext)
   {
      var services = new ServiceCollection();
      services.AddScoped(_ => unitOfWork);
      services.AddScoped(_ => userContext);
      return services.BuildServiceProvider();
   }

   private class FakeSentinelUnitOfWork(FakeSystemLogRepository systemLogs) : ISentinelUnitOfWork
   {
      public int SaveChangesCount { get; private set; }
      public IAuditLogRepository AuditLogs => throw new NotImplementedException();
      public ISystemLogRepository SystemLogs => systemLogs;

      public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      {
         SaveChangesCount++;
         return Task.FromResult(1);
      }

      public void Dispose()
      {
      }
   }

   private class FakeSystemLogRepository : ISystemLogRepository
   {
      public List<SystemLog> Logs { get; } = [];
      public bool ShouldThrow { get; init; }

      public Task AddAsync(SystemLog log, CancellationToken cancellationToken = default)
      {
         if (ShouldThrow)
         {
            throw new InvalidOperationException("Mongo unavailable");
         }

         Logs.Add(log);
         return Task.CompletedTask;
      }

      public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
      {
         return Task.FromResult(Logs.Any(log => log.Id == id));
      }
   }

   private class FakeUserContext(Guid userId, Guid organizationId) : IUserContext
   {
      public Guid UserId { get; } = userId;
      public Guid OrganizationId { get; } = organizationId;
      public bool IsSystemAdmin => false;
      public bool IsSupportUser => false;
      public bool IsOrganizationAdmin => false;
      public bool IsAuthenticated => true;
      public string? Language => "en";
      public string? IpAddress => "127.0.0.1";
      public string? UserAgent => "test-agent";
      public IEnumerable<string> Roles => [];
   }
}
