using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain.Messages;

namespace Shared.Application.Services;

public class BaseService
{
   protected readonly IUserContext _userContext;

   protected BaseService(IUserContext userContext)
   {
      _userContext = userContext;
   }

   /// <summary>
   /// Validates resource ownership before executing a task that returns a standard <see cref="Result"/>.
   /// </summary>
   /// <param name="resourceOwnerId">The unique identifier of the resource owner to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <returns>
   /// A <see cref="Result"/> indicating success and executing the action, 
   /// or a failure result containing an <see cref="UnauthorizedAccessError"/> if validation fails.
   /// </returns>
   protected async Task<Result> ExecuteIfUserOwnsAsync(Guid? resourceOwnerId, Func<CancellationToken, Task<Result>> actionAsync, CancellationToken cancellationToken = default)
   {
      if (!IsUserAlllowedToAccess(resourceOwnerId))
      {
         return Result.Failure(new UnauthorizedAccessError());
      }

      return await actionAsync(cancellationToken);
   }

   /// <summary>
   /// Validates resource ownership before executing a task that returns a specialized <typeparamref name="TResult"/>.
   /// </summary>
   /// <typeparam name="TResult">A type that inherits from <see cref="Result"/>.</typeparam>
   /// <param name="resourceOwnerId">The unique identifier of the resource owner to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <returns>
   /// The <typeparamref name="TResult"/> produced by the action, 
   /// or a new instance of <typeparamref name="TResult"/> with an <see cref="UnauthorizedAccessError"/> message if validation fails.
   /// </returns>
   protected async Task<TResult> ExecuteIfUserOwnsAsync<TResult>(Guid? resourceOwnerId, Func<CancellationToken, Task<TResult>> actionAsync, CancellationToken cancellationToken = default) where TResult : Result
   {
      if (!IsUserAlllowedToAccess(resourceOwnerId))
      {
         var result = Activator.CreateInstance<TResult>()!;

         result.AddMessage(new UnauthorizedAccessError());

         return result;
      }

      return await actionAsync(cancellationToken);
   }

   /// <summary>
   /// Validates resource ownership before executing a task that returns a single object.
   /// </summary>
   /// <typeparam name="T">The object type to be returned.</typeparam>
   /// <param name="resourceOwnerId">The unique identifier of the resource owner to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns></returns>
   protected async Task<T?> ExecuteIfUserOwnSingleObjectAsync<T>(Guid? resourceOwnerId, Func<CancellationToken, Task<T?>> actionAsync, CancellationToken cancellationToken = default)
   {
      if (!IsUserAlllowedToAccess(resourceOwnerId))
      {
         return default;
      }

      return await actionAsync(cancellationToken);
   }
   /// <summary>
   /// Validates resorce ownership before executing a task that returns a collection of objects.
   /// </summary>
   /// <typeparam name="T">The collection type to be returned.</typeparam>
   /// <param name="resourceOwnerId">The unique identifier of the resource owner to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns></returns>

   protected async Task<IEnumerable<T>> ExecuteIfUserOwnsCollectionAsync<T>(Guid? resourceOwnerId, Func<CancellationToken, Task<IEnumerable<T>>> actionAsync, CancellationToken cancellationToken = default)
   {
      if (!IsUserAlllowedToAccess(resourceOwnerId))
      {
         return [];
      }

      return await actionAsync(cancellationToken);
   }

   private bool IsUserAlllowedToAccess(Guid? resourceOwnerId)
   {
      return _userContext.IsSystemAdmin ||
             (resourceOwnerId.HasValue && resourceOwnerId == _userContext.UserOwnerId);
   }
}
