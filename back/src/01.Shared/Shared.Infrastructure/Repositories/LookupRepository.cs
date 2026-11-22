using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Entities;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Repositories;

internal class LookupRepository(SharedDbContext dbContext) : BaseRepository<Lookup>(dbContext), ILookupRepository
{
   private const string AcquireWriteLockSql = "SELECT pg_advisory_xact_lock(1936220528, 1)";
   // Serialize lookup commands before checking cross-row hierarchy and uniqueness rules.
   // Validation and persistence share one transaction, including across API instances.
   public async Task<ILookupWriteTransaction> BeginWriteAsync(CancellationToken cancellationToken = default)
   {
      var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
      try
      {
         await dbContext.Database.ExecuteSqlRawAsync(AcquireWriteLockSql, cancellationToken);
         return new WriteTransaction(transaction);
      }
      catch
      {
         await transaction.DisposeAsync();
         throw;
      }
   }

   public override void Update(Lookup entity)
   {
      // The aggregate is already tracked. Preserve translation creation audit fields.
      dbContext.Entry(entity).State = EntityState.Modified;
   }

   private sealed class WriteTransaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
      : ILookupWriteTransaction
   {
      public Task CommitAsync(CancellationToken cancellationToken = default) => transaction.CommitAsync(cancellationToken);
      public ValueTask DisposeAsync() => transaction.DisposeAsync();
   }

   public override Task<Lookup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
      dbContext.Lookups.Include(x => x.Translations).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

   public async Task<bool> HasDuplicateTranslationAsync(string type, Guid? parentId, Guid? organizationId,
      IReadOnlyList<LookupTranslationRequest> translations, Guid? excludedId = null, CancellationToken cancellationToken = default)
   {
      var normalizedType = type.Trim().ToLowerInvariant();
      var query = dbContext.LookupTranslations.AsNoTracking().Where(t =>
         t.Lookup.Type == normalizedType && t.Lookup.ParentId == parentId &&
         t.Lookup.OrganizationId == organizationId && (!excludedId.HasValue || t.LookupId != excludedId));
      foreach (var translation in translations)
      {
         var language = LanguageOptions.Normalize(translation.LanguageCode);
         var code = translation.Code.Trim();
         if (await query.AnyAsync(t => t.LanguageCode == language && t.Code == code, cancellationToken)) return true;
      }
      return false;
   }

   public async Task<bool> WouldCreateCycleAsync(Guid id, Guid? parentId, CancellationToken cancellationToken = default)
   {
      var visited = new HashSet<Guid> { id };
      while (parentId.HasValue)
      {
         if (!visited.Add(parentId.Value)) return true;
         parentId = await dbContext.Lookups.AsNoTracking().Where(x => x.Id == parentId.Value)
            .Select(x => x.ParentId).SingleOrDefaultAsync(cancellationToken);
      }
      return false;
   }
}
