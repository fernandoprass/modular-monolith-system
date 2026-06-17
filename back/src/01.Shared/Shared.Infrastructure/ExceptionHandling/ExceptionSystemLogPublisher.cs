using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Application.Contracts;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.ExceptionHandling;

public class ExceptionSystemLogPublisher(
   IEventPublisher eventPublisher,
   IUserContext userContext,
   ILogger<ExceptionSystemLogPublisher> logger) : IExceptionSystemLogPublisher
{
   private readonly IEventPublisher _eventPublisher = eventPublisher;
   private readonly IUserContext _userContext = userContext;
   private readonly ILogger<ExceptionSystemLogPublisher> _logger = logger;

   public async Task PublishAsync(
      string module,
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken = default)
   {
      try
      {
         var systemLogEvent = SystemLogEventFactory.Create(module, httpContext, exception, _userContext);
         await _eventPublisher.PublishSystemLogEventAsync(systemLogEvent, cancellationToken);
      }
      catch (Exception publishException)
      {
         _logger.LogError(publishException, "Failed to publish exception system log for {Module}", module);
      }
   }
}
