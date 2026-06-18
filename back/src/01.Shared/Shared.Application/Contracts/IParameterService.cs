using Myce.Response;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;

namespace Shared.Application.Contracts
{
   public interface IParameterService
   {
      //Method to be used in the UI for management of parameters, not intended for use in code
      //to get parameter values, for that use the GetByKeyAsync and GetValueAsync methods
      Task<Result<ParameterDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
      Task<PagedResultDto<ParameterLiteDto>> GetAsync(ParameterSearchRequest request, CancellationToken cancellationToken = default);
      Task<Result<IEnumerable<ParameterLiteDto>>> GetOwnerIdAsync(ParameterOverrideType overrideType, CancellationToken cancellationToken = default);
      Task<Result<ParameterValueDto>> GetValueAsync(string key, CancellationToken cancellationToken = default);
      Task<Result> SaveOverrideValueAsync(Guid parameterId, ParameterOwnerUpdateRequest request, CancellationToken cancellationToken = default);
      Task<Result> DeleteOverrideValueAsync(Guid parameterOverrideId, CancellationToken cancellationToken = default);

      //Convenience methods to get parameter values directly by key, will throw an exception if the
      //parameter is not found or if the value cannot be converted to the expected type
      Task<Result<ParameterDto>> CreateAsync(ParameterCreateRequest request, CancellationToken cancellationToken = default);
      Task<Result> UpdateAsync(Guid id, ParameterUpdateRequest request, CancellationToken cancellationToken = default);
      Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
      Task<ParameterDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
      Task<bool> GetBoolAsync(string key, CancellationToken cancellationToken = default);
      Task<short> GetShortIntAsync(string key, CancellationToken cancellationToken = default);
      Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default);
      Task<long> GetLongIntAsync(string key, CancellationToken cancellationToken = default);
      Task<double> GetDoubleAsync(string key, CancellationToken cancellationToken = default);
      Task<decimal> GetDecimalAsync(string key, CancellationToken cancellationToken = default);
      Task<DateTime> GetDateTimeAsync(string key, CancellationToken cancellationToken = default);
      Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default);
      Task<Guid> GetGuidAsync(string key, CancellationToken cancellationToken = default);

   }
}
