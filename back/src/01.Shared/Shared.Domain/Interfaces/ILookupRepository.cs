using Shared.Domain.Entities;
using Shared.Domain.DTOs.Requests;

namespace Shared.Domain.Interfaces;

internal interface ILookupRepository : IBaseRepository<Lookup, Guid>
{
   Task<ILookupWriteTransaction> BeginWriteAsync(CancellationToken cancellationToken = default);
   Task<bool> HasDuplicateTranslationAsync(string type, Guid? parentId, Guid? organizationId,
      IReadOnlyList<LookupTranslationRequest> translations, Guid? excludedId = null, CancellationToken cancellationToken = default);
   Task<bool> WouldCreateCycleAsync(Guid id, Guid? parentId, CancellationToken cancellationToken = default);
}
