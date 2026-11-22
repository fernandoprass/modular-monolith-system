using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;

namespace Shared.Domain.Interfaces;

internal interface ILookupQueryRepository
{
   Task<LookupDto?> GetByIdAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default);
   Task<PagedResultDto<LookupLiteDto>> GetAsync(LookupSearchRequest request, Guid organizationId,
      CancellationToken cancellationToken = default);
}
