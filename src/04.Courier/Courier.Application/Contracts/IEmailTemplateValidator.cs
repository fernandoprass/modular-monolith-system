using Courier.Domain.DTOs.Requests;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface IEmailTemplateValidator
{
   Result ValidateCreate(EmailTemplateCreateRequest request, bool keyExists);
   Result ValidateUpdate(EmailTemplateUpdateRequest request, bool templateExists, bool keyExists);
   Result ValidateSearch(EmailTemplateSearchRequest request);
   Result ValidateTranslation(EmailTemplateTranslationRequest request);
}
