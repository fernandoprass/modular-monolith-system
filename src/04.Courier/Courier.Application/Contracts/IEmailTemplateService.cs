using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface IEmailTemplateService
{
   Task<Result<PagedResultDto<EmailTemplateDto>>> GetAsync(EmailTemplateSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<EmailTemplateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result<EmailTemplateDto>> CreateAsync(EmailTemplateCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, EmailTemplateUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result> AddTranslationAsync(Guid id, EmailTemplateTranslationRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateTranslationAsync(Guid id, string language, EmailTemplateTranslationRequest request, CancellationToken cancellationToken = default);
   Task<Result> RemoveTranslationAsync(Guid id, string language, CancellationToken cancellationToken = default);
}
