using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;

namespace Courier.Domain.Mappers;

public static class EmailMappers
{
   public static EmailDto ToEmailDto(this Email email)
   {
      return new EmailDto(
         email.Id,
         email.OrganizationId,
         email.UserId,
         email.Module,
         email.Feature,
         email.TemplateKey,
         email.Recipient,
         email.Subject,
         email.Body,
         email.IsHtml,
         email.CreatedAt,
         email.SentAt,
         email.ExpiresAt,
         email.Status,
         email.RetryCount,
         email.NextAttemptAt,
         email.Attempts);
   }

   public static EmailLiteDto ToEmailLiteDto(this Email email)
   {
      return new EmailLiteDto(
         email.Id,
         email.Module,
         email.Feature,
         email.Recipient,
         email.Subject,
         email.Status);
   }
}
