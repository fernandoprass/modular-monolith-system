using Myce.Response;
using Shared.Domain.DTOs.Requests;

namespace Shared.Application.Contracts;

internal interface ILookupValidator
{
   Result ValidateCreate(LookupCreateRequest request);
   Result ValidateUpdate(LookupUpdateRequest request);
   Result ValidateSearch(LookupSearchRequest request);
   Result ValidateRelations(bool parentExists, bool sameScope, bool cycle, bool duplicate);
}
