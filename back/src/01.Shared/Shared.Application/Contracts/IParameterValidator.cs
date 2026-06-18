using Myce.Response;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Entities;

namespace Shared.Application.Contracts;

internal interface IParameterValidator
{
   Result ValidateCreate(ParameterCreateRequest request, bool keyExists);
   Result ValidateUpdate(bool parameterExists, bool keyExists, ParameterUpdateRequest request);
   Result ValidateOwnerUpdate(Parameter? parameter, ParameterOwnerUpdateRequest request);
}
