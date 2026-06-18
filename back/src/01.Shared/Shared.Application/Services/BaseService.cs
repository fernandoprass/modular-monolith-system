using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Enums;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Domain.Messages;
using System.Text.Json;

namespace Shared.Application.Services;

public class BaseService
{
   protected readonly IUserContext _userContext;
   private readonly IEventPublisher? _eventPublisher;

   protected BaseService(IUserContext userContext, IEventPublisher? eventPublisher = null)
   {
      _userContext = userContext;
      _eventPublisher = eventPublisher;
   }

   /// <summary>
   /// Validates resource ownership before executing a task that returns a standard <see cref="Result"/>.
   /// </summary>
   /// <param name="organizationId">The unique identifier of the organization to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <returns>
   /// A <see cref="Result"/> indicating success and executing the action, 
   /// or a failure result containing an <see cref="UnauthorizedAccessError"/> if validation fails.
   /// </returns>
   protected async Task<Result> ExecuteIfUserOwnsAsync(Guid? organizationId, Func<CancellationToken, Task<Result>> actionAsync, CancellationToken cancellationToken = default)
   {
      if (!IsUserAlllowedToAccess(organizationId))
      {
         await PublishUnauthorizedResourceAccessAuditLogAsync(organizationId, cancellationToken);
         return Result.Failure(new UnauthorizedAccessError());
      }

      return await actionAsync(cancellationToken);
   }

   /// <summary>
   /// Validates resource ownership before executing a task that returns a specialized <typeparamref name="TResult"/>.
   /// </summary>
   /// <typeparam name="TResult">A type that inherits from <see cref="Result"/>.</typeparam>
   /// <param name="organizationId">The unique identifier of the organization to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <returns>
   /// The <typeparamref name="TResult"/> produced by the action, 
   /// or a new instance of <typeparamref name="TResult"/> with an <see cref="UnauthorizedAccessError"/> message if validation fails.
   /// </returns>
   protected async Task<TResult> ExecuteIfUserOwnsAsync<TResult>(Guid? organizationId, Func<CancellationToken, Task<TResult>> actionAsync, CancellationToken cancellationToken = default) where TResult : Result
   {
      if (!IsUserAlllowedToAccess(organizationId))
      {
         await PublishUnauthorizedResourceAccessAuditLogAsync(organizationId, cancellationToken);
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
   /// <param name="organizationId">The unique identifier of the organization to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns></returns>
   protected async Task<T?> ExecuteIfUserOwnSingleObjectAsync<T>(Guid? organizationId, Func<CancellationToken, Task<T?>> actionAsync, CancellationToken cancellationToken = default)
   {
      if (!IsUserAlllowedToAccess(organizationId))
      {
         await PublishUnauthorizedResourceAccessAuditLogAsync(organizationId, cancellationToken);
         return default;
      }

      return await actionAsync(cancellationToken);
   }
   /// <summary>
   /// Validates resorce ownership before executing a task that returns a collection of objects.
   /// </summary>
   /// <typeparam name="T">The collection type to be returned.</typeparam>
   /// <param name="organizationId">The unique identifier of the organization to be validated against the current user context.</param>
   /// <param name="actionAsync">The asynchronous function to execute if ownership validation succeeds.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns></returns>

   protected async Task<IEnumerable<T>> ExecuteIfUserOwnsCollectionAsync<T>(Guid? organizationId, Func<CancellationToken, Task<IEnumerable<T>>> actionAsync, CancellationToken cancellationToken = default)
   {
      if (!IsUserAlllowedToAccess(organizationId))
      {
         await PublishUnauthorizedResourceAccessAuditLogAsync(organizationId, cancellationToken);
         return [];
      }

      return await actionAsync(cancellationToken);
   }

   private bool IsUserAlllowedToAccess(Guid? organizationId)
   {
      return _userContext.IsSystemAdmin ||
             (organizationId.HasValue && organizationId == _userContext.OrganizationId);
   }

   private async Task PublishUnauthorizedResourceAccessAuditLogAsync(Guid? resourceOwnerId, CancellationToken cancellationToken)
   {
      if (_eventPublisher is null)
      {
         return;
      }

      var metadata = new
      {
         ResourceOwnerId = resourceOwnerId,
         _userContext.OrganizationId,
         _userContext.UserId
      };

      var auditLogEvent = AuditLogEvent.Create(
         module: SharedConst.System.ModuleName.ToLowerInvariant(),
         feature: SharedConst.Logger.Feature.Security,
         action: SharedConst.Logger.Action.UnauthorizedResourceAccess,
         description: "User tried to access a resource owned by another tenant.",
         privacyLevel: AuditPrivacyLevel.High,
         retentionPolicy: RetentionPolicy.Compliance,
         ipAddress: _userContext.IpAddress,
         userAgent: _userContext.UserAgent,
         userId: _userContext.UserId,
         targetId: resourceOwnerId ?? Guid.Empty,
         organizationId: _userContext.OrganizationId,
         metadata: JsonSerializer.Serialize(metadata));

      await _eventPublisher.PublishAuditLogEventAsync(auditLogEvent, cancellationToken);
   }
}
