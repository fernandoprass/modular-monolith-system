using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Myce.Response;
using Shared.Domain.DTOs.Responses;

namespace Courier.Application.Contracts;

public interface ITemplateService
{
   Task<Result<PagedResultDto<TemplateLiteDto>>> GetAsync(TemplateSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<TemplateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result<TemplateDto>> CreateAsync(TemplateCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, TemplateUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> AddEmailTranslationAsync(Guid id, TemplateEmailTranslationRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateEmailTranslationAsync(Guid id, string language, TemplateEmailTranslationRequest request, CancellationToken cancellationToken = default);
   Task<Result> RemoveTranslationAsync(Guid id, string language, CancellationToken cancellationToken = default);
}
