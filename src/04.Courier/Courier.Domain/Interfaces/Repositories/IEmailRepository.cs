using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;

namespace Courier.Domain.Interfaces.Repositories;

public interface IEmailRepository
{
   Task<PagedResultDto<EmailLiteDto>> GetAsync(EmailSearchRequest request, CancellationToken cancellationToken = default);
   Task<Email?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Guid> AddAsync(Email email, CancellationToken cancellationToken = default);
}
