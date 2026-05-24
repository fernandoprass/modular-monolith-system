namespace Shared.Domain.Events;

public record EmailRequestedEvent(
   Guid OrganizationId,
   Guid UserId,
   string Module,
   string Feature,
   string TemplateKey,
   string Language,
   string Recipient,
   IReadOnlyDictionary<string, string>? Values = null);
