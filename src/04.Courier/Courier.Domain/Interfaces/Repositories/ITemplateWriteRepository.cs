using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;

namespace Courier.Domain.Interfaces.Repositories;

public interface ITemplateWriteRepository
{
   Task<PagedResultDto<TemplateLiteDto>> GetAsync(TemplateSearchRequest request, CancellationToken cancellationToken = default);
   Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<bool> KeyExistsAsync(string key, Guid? excludedId = null, CancellationToken cancellationToken = default);
   Task<Guid> AddAsync(Template template, CancellationToken cancellationToken = default);
   Task UpdateAsync(Template template, CancellationToken cancellationToken = default);
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
