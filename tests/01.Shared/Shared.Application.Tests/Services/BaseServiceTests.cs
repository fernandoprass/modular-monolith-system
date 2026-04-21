using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.Messages;

namespace Shared.Application.Tests.Services;

public class TestService : BaseService
{
   public TestService(IUserContext userContext) : base(userContext) { }

   public async Task<Result> TestExecuteIfUserOwnsAsync(Guid? resourceOrganizationId, Func<CancellationToken, Task<Result>> action, CancellationToken cancellationToken = default)
       => await ExecuteIfUserOwnsAsync(resourceOrganizationId, action, cancellationToken);

   public async Task<TResult> TestExecuteIfUserOwnsAsyncGeneric<TResult>(Guid? resourceOrganizationId, Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default) where TResult : Result
       => await ExecuteIfUserOwnsAsync<TResult>(resourceOrganizationId, action, cancellationToken);
}

public class BaseServiceTests
{
   private readonly IUserContext _userContextMock;
   private readonly TestService _service;

   public BaseServiceTests()
   {
      _userContextMock = Substitute.For<IUserContext>();
      _service = new TestService(_userContextMock);
   }

   [Fact]
   public async Task ExecuteIfUserOwnsAsync_ShouldAllowForDifferentOrganization_WhenUserIsSystemAdmin()
   {
      _userContextMock.IsSystemAdmin.Returns(true);
      var resourceOrganizationId = Guid.NewGuid();
      var actionCalled = false;

      var result = await _service.TestExecuteIfUserOwnsAsync(resourceOrganizationId, (ct) =>
      {
         actionCalled = true;
         return Task.FromResult(Result.Success());
      }, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.True(actionCalled);
   }

   [Fact]
   public async Task ExecuteIfUserOwnsAsync_ShouldAllow_WhenUserOwnsTheResource()
   {
      var myOrganizationId = Guid.NewGuid();
      _userContextMock.IsSystemAdmin.Returns(false);
      _userContextMock.UserOwnerId.Returns(myOrganizationId);

      var actionCalled = false;

      var result = await _service.TestExecuteIfUserOwnsAsync(myOrganizationId, (ct) =>
      {
         actionCalled = true;
         return Task.FromResult(Result.Success());
      }, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.True(actionCalled);
   }

   [Fact]
   public async Task ExecuteIfUserOwnsAsync_ShouldFail_WhenUserIsNotOwnerAndNotAdmin()
   {
      _userContextMock.IsSystemAdmin.Returns(false);
      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());

      var targetOwnerId = Guid.NewGuid(); // Different id
      var actionCalled = false;

      var result = await _service.TestExecuteIfUserOwnsAsync(targetOwnerId, (ct) =>
      {
         actionCalled = true;
         return Task.FromResult(Result.Success());
      }, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.False(actionCalled);
      Assert.IsType<UnauthorizedAccessError>(result.Messages.First());
   }

   [Fact]
   public async Task ExecuteIfUserOwnsAsync_Generic_ShouldReturnCorrectTypeOnFailure()
   {
      _userContextMock.IsSystemAdmin.Returns(false);
      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());

      var result = await _service.TestExecuteIfUserOwnsAsyncGeneric<Result<string>>(Guid.NewGuid(), (ct) =>
          Task.FromResult(Result<string>.Success("Should not be called")), TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
      Assert.IsType<Result<string>>(result);
      Assert.IsType<UnauthorizedAccessError>(result.Messages.First());
   }
}
