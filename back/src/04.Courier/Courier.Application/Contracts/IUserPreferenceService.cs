using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface IUserPreferenceService
{
   Task<Result<IReadOnlyCollection<UserPreferenceTemplateOptionDto>>> GetAsync(
      CancellationToken cancellationToken = default);
   Task<Result> UpdateAsync(UserPreferenceUpdateRequest request, CancellationToken cancellationToken = default);
}
