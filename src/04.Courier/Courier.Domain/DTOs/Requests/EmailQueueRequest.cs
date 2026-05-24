namespace Courier.Domain.DTOs.Requests;

public record EmailQueueRequest(
   Guid OrganizationId,
   Guid UserId,
   string Module,
   string Feature,
   string TemplateKey,
   string Language,
   string Recipient,
   IReadOnlyDictionary<string, string>? Values = null);
