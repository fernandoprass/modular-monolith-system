using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using MongoDB.Driver;
using Shared.Domain;

namespace Courier.Infrastructure.Repositories;

public class UserPreferenceRepository(CourierDbContext dbContext) : IUserPreferenceRepository
{
   private readonly CourierDbContext _dbContext = dbContext;

   public async Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      return await _dbContext.UserPreferences
         .Find(p => p.UserId == userId)
         .SingleOrDefaultAsync(cancellationToken);
   }

   public async Task<IReadOnlyCollection<UserPreferenceTemplateOptionDto>> GetOptOutTemplateOptionsAsync(
      string language,
      CancellationToken cancellationToken = default)
   {
      var normalizedLanguage = LanguageOptions.Normalize(language);
      var templates = await _dbContext.Templates
         .Find(t => t.IsAllowingOptOut)
         .SortBy(t => t.Module)
         .ThenBy(t => t.Key)
         .ToListAsync(cancellationToken);

      return templates
         .Select(template =>
         {
            var name = template.Translations
               .SingleOrDefault(translation => translation.Language == normalizedLanguage)
               ?.Name ?? string.Empty;

            return new UserPreferenceTemplateOptionDto(
               template.Module,
               template.Key,
               name,
               true,
               true);
         })
         .ToArray();
   }

   public async Task<Guid> AddAsync(UserPreference preference, CancellationToken cancellationToken = default)
   {
      await _dbContext.UserPreferences.InsertOneAsync(preference, cancellationToken: cancellationToken);
      return preference.Id;
   }

   public async Task UpdateAsync(UserPreference preference, CancellationToken cancellationToken = default)
   {
      await _dbContext.UserPreferences.ReplaceOneAsync(
         p => p.UserId == preference.UserId,
         preference,
         new ReplaceOptions { IsUpsert = true },
         cancellationToken);
   }
}
