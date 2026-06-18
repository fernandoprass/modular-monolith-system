namespace Courier.Domain.DTOs.Requests;

public record EmailCreateRequest(
   Guid OrganizationId,
   Guid UserId,
   string Module,
   string Feature,
   string TemplateKey,
   string Recipient,
   string Subject,
   string Body,
   bool IsHtml);
