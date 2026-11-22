using Myce.Response;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;

namespace Shared.Application.Contracts;

public interface ILookupService
{
   Task<Result<LookupDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<Result<PagedResultDto<LookupLiteDto>>> GetAsync(LookupSearchRequest request, CancellationToken cancellationToken = default);
   Task<Result<LookupDto>> CreateAsync(LookupCreateRequest request, CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(Guid id, LookupUpdateRequest request, CancellationToken cancellationToken = default);
   Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
