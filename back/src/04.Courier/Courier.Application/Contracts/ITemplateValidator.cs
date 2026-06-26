using Courier.Domain.DTOs.Requests;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface ITemplateValidator
{
   Result ValidateCreate(TemplateCreateRequest request, bool keyExists);
   Result ValidateUpdate(TemplateUpdateRequest request, bool templateExists, bool keyExists);
   Result ValidateSearch(TemplateSearchRequest request);
   Result ValidateTranslation(TemplateTranslationRequest request, bool templateExists);
}
