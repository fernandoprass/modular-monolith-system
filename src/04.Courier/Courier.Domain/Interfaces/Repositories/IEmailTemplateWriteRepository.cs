using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;

namespace Courier.Domain.Interfaces.Repositories;

public interface IEmailTemplateWriteRepository
{
   Task<PagedResultDto<EmailTemplateDto>> GetAsync(EmailTemplateSearchRequest request, CancellationToken cancellationToken = default);
   Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<bool> KeyExistsAsync(string key, Guid? excludedId = null, CancellationToken cancellationToken = default);
   Task<Guid> AddAsync(EmailTemplate template, CancellationToken cancellationToken = default);
   Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
