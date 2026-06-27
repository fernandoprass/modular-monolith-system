using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;

namespace Courier.Domain.Interfaces.Repositories;

public interface IUserPreferenceRepository
{
   Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
   Task<IReadOnlyCollection<UserPreferenceTemplateOptionDto>> GetOptOutTemplateOptionsAsync(
      string language,
      CancellationToken cancellationToken = default);
   Task<Guid> AddAsync(UserPreference preference, CancellationToken cancellationToken = default);
   Task UpdateAsync(UserPreference preference, CancellationToken cancellationToken = default);
}
