namespace Shared.Domain.Events;

public record UserMessageEvent(
   Guid OrganizationId,
   Guid UserId,
   string Module,
   string Feature,
   string TemplateKey,
   string Language,
   string? Recipient,
   IReadOnlyDictionary<string, string>? Values = null);
