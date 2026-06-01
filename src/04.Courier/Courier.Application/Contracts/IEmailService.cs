using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Myce.Response;
using Shared.Domain.DTOs.Responses;

namespace Courier.Application.Contracts;

public interface IEmailService
{
   Task<Result<PagedResultDto<EmailLiteDto>>> GetAsync(EmailSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<EmailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result<EmailCreateDto>> CreateAsync(EmailCreateRequest request, CancellationToken cancellationToken = default);
}
