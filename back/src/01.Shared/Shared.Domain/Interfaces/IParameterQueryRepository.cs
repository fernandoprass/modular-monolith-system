using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;

namespace Shared.Domain.Interfaces;

internal interface IParameterQueryRepository
{
   Task<ParameterDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task<IEnumerable<ParameterLiteDto>> GetAllAsync(ParameterSearchRequestInternal request, CancellationToken cancellationToken = default);
   Task<ParameterDto?> GetByModuleGroupAndKeyAsync(string module, string group, string name, CancellationToken cancellationToken = default);
   Task<ParameterValueDto?> GetValueAsync(string key, Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
}
