using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Entities;
using Shared.Domain.Interfaces;
using Shared.Domain.Mappers;
using Shared.Domain.Messages;

namespace Shared.Application.Services;

internal class LookupService(
   ISharedUnitOfWork unitOfWork, ILookupRepository repository, ILookupQueryRepository queryRepository,
   ILookupValidator validator, IUserContext userContext, IEventPublisher eventPublisher,
   LookupAuditLogger auditLogger) : BaseService(userContext, eventPublisher), ILookupService
{
   public async Task<Result<LookupDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var lookup = await queryRepository.GetByIdAsync(id, _userContext.OrganizationId, cancellationToken);
      return lookup == null 
         ? Result<LookupDto>.Failure(new NotFoundError(SharedConst.Entity.Lookup))
         : Result<LookupDto>.Success(lookup);
   }

   public async Task<Result<PagedResultDto<LookupLiteDto>>> GetAsync(LookupSearchRequest request, CancellationToken cancellationToken = default)
   {
      var validation = validator.ValidateSearch(request);
      if (validation.HasError)
      return Result<PagedResultDto<LookupLiteDto>>.Failure(validation.Messages);

      var normalized = request with { LanguageCode = request.LanguageCode ?? _userContext.Language };
      return Result<PagedResultDto<LookupLiteDto>>.Success(
         await queryRepository.GetAsync(normalized, _userContext.OrganizationId, cancellationToken));
   }

   public Task<Result<LookupDto>> CreateAsync(LookupCreateRequest request, CancellationToken cancellationToken = default) =>
      ExecuteIfUserOwnsAsync<Result<LookupDto>>(request.OrganizationId, async ct =>
      {
         var validation = validator.ValidateCreate(request);
         if (validation.HasError) return Result<LookupDto>.Failure(validation.Messages);
         await using var transaction = await repository.BeginWriteAsync(ct);
         validation = await ValidateRelationsAsync(null, request.Type, request.ParentId, request.OrganizationId, request.Translations, ct);
         if (validation.HasError) 
         {
            return Result<LookupDto>.Failure(validation.Messages);
         }
         var lookup = Lookup.Create(request);
         await unitOfWork.Lookups.AddAsync(lookup, ct);
         await unitOfWork.SaveChangesAsync(ct);
         await transaction.CommitAsync(ct);
         await auditLogger.LogAsync(lookup, SharedConst.Logger.Action.Create, ct);
         return Result<LookupDto>.Success(lookup.ToLookupDto());
      }, cancellationToken);

   public async Task<Result> UpdateAsync(Guid id, LookupUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var validation = validator.ValidateUpdate(request);
      if (validation.HasError) return Result.Failure(validation.Messages);
      await using var transaction = await repository.BeginWriteAsync(cancellationToken);
      var lookup = await repository.GetByIdAsync(id, cancellationToken);
      if (lookup == null) return Result.Failure(new NotFoundError(SharedConst.Entity.Lookup));
      return await ExecuteIfUserOwnsAsync(lookup.OrganizationId, async ct =>
      {
         var relations = await ValidateRelationsAsync(id, lookup.Type, request.ParentId, lookup.OrganizationId, request.Translations, ct);
         if (relations.HasError) 
         {
            return Result.Failure(relations.Messages);
         }
         lookup.Update(request);
         repository.Update(lookup);
         await unitOfWork.SaveChangesAsync(ct);
         await transaction.CommitAsync(ct);
         await auditLogger.LogAsync(lookup, SharedConst.Logger.Action.Update, ct);
         return Result.Success();
      }, cancellationToken);
   }

   public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      await using var transaction = await repository.BeginWriteAsync(cancellationToken);
      var lookup = await repository.GetByIdAsync(id, cancellationToken);
      if (lookup == null)
      {
         return Result.Failure(new NotFoundError(SharedConst.Entity.Lookup));
      }
      return await ExecuteIfUserOwnsAsync(lookup.OrganizationId, async ct =>
      {
         await repository.DeleteAsync(id, ct);
         await unitOfWork.SaveChangesAsync(ct);
         await transaction.CommitAsync(ct);
         await auditLogger.LogAsync(lookup, SharedConst.Logger.Action.Delete, ct);
         return Result.Success();
      }, cancellationToken);
   }

   private async Task<Result> ValidateRelationsAsync(Guid? id, string type, Guid? parentId, Guid? organizationId,
      IReadOnlyList<LookupTranslationRequest> translations, CancellationToken cancellationToken = default)
   {
      var parent = parentId.HasValue ? await repository.GetByIdAsync(parentId.Value, cancellationToken) : null;
      var parentExists = !parentId.HasValue || parent != null;
      var sameScope = !parentId.HasValue || parent?.OrganizationId == organizationId;
      var cycle = id.HasValue && await repository.WouldCreateCycleAsync(id.Value, parentId, cancellationToken);
      var duplicate = await repository.HasDuplicateTranslationAsync(type, parentId, organizationId, translations, id, cancellationToken);
      return validator.ValidateRelations(parentExists, sameScope, cycle, duplicate);
   }
}
