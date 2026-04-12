using Shared.Domain.Entities;

namespace Shared.Domain.Interfaces;

internal interface IParameterOverrideRepository
{
   Task<ParameterOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task AddAsync(ParameterOverride parameterOverride, CancellationToken cancellationToken = default);
   void Update(ParameterOverride parameterOverride);
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<ParameterOverride?> GetByParameterIdAndOwnerIdAsync(Guid parameterId, Guid ownerId, CancellationToken cancellationToken = default);
}
