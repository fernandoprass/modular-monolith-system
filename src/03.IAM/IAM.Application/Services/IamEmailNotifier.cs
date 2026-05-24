using IAM.Application.Contracts;
using IAM.Domain;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;

namespace IAM.Application.Services;

public class IamEmailNotifier(
   IEventPublisher eventPublisher,
   IUserContext userContext) : IIamEmailNotifier
{
   private readonly IEventPublisher _eventPublisher = eventPublisher;
   private readonly IUserContext _userContext = userContext;

   public async Task NotifyAsync(
      string templateKey,
      Guid organizationId,
      Guid userId,
      string recipient,
      string feature,
      IReadOnlyDictionary<string, string>? values = null,
      CancellationToken cancellationToken = default)
   {
      var emailRequest = new EmailRequestedEvent(
         organizationId,
         userId,
         IamConst.System.ModuleName.ToLowerInvariant(),
         feature,
         templateKey,
         GetLanguage(),
         recipient,
         values);

      await _eventPublisher.PublishEmailRequestedEventAsync(emailRequest, cancellationToken);
   }

   private string GetLanguage()
   {
      return string.IsNullOrWhiteSpace(_userContext.Language)
         ? SharedConst.System.DefaultLanguage
         : _userContext.Language.Trim().ToLowerInvariant();
   }
}
