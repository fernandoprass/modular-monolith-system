using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Shared.Domain.DTOs.Responses;

namespace Courier.Domain.Interfaces.Repositories;

public interface IEmailRepository
{
   Task<PagedResultDto<EmailLiteDto>> GetAsync(EmailSearchRequest request, CancellationToken cancellationToken = default);
   Task<Email?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Email?> ClaimNextPendingAsync(DateTime utcNow, CancellationToken cancellationToken = default);
   Task<Guid> AddAsync(Email email, CancellationToken cancellationToken = default);
   Task UpdateAsync(Email email, CancellationToken cancellationToken = default);
}
