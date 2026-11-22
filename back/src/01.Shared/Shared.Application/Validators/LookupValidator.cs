using Myce.FluentValidator;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Messages;

namespace Shared.Application.Validators;

internal class LookupValidator : ILookupValidator
{
   public Result ValidateCreate(LookupCreateRequest request)
   {
      var validator = new FluentValidator<LookupCreateRequest>()
         .RuleFor(x => x.Type).IsRequired().MaxLength(SharedConst.Lookup.TypeMaxLength);
      if (!validator.Validate(request)) return Result.Failure(validator.Messages);
      if (request.OrganizationId == Guid.Empty || request.ParentId == Guid.Empty)
         return Result.Failure(new LookupInvalidParentError());
      return ValidateTranslations(request.Translations);
   }

   public Result ValidateUpdate(LookupUpdateRequest request)
   {
      if (request.ParentId == Guid.Empty) return Result.Failure(new LookupInvalidParentError());
      return ValidateTranslations(request.Translations);
   }

   public Result ValidateSearch(LookupSearchRequest request)
   {
      var validator = new FluentValidator<LookupSearchRequest>()
         .RuleFor(x => x.PageNumber).IsGreaterThan(0)
         .RuleFor(x => x.PageSize).IsGreaterThan(0)
         .RuleForValue(((long)request.PageNumber - 1) * Math.Min(request.PageSize, SharedConst.Pagination.MaxPageSize) <= int.MaxValue)
            .IsTrue(new LookupInvalidPageError());
      if (!validator.Validate(request)) return Result.Failure(validator.Messages);
      if (request.RootsOnly && request.ParentId.HasValue)
         return Result.Failure(new LookupInvalidSearchError());
      if (request.LanguageCode != null && !LanguageOptions.IsSupported(request.LanguageCode))
         return Result.Failure(new InvalidLanguageError(request.LanguageCode));
      return Result.Success();
   }

   public Result ValidateRelations(bool parentExists, bool sameScope, bool cycle, bool duplicate)
   {
      var validator = new FluentValidator<LookupUpdateRequest>()
         .RuleForValue(parentExists && sameScope && !cycle).IsTrue(new LookupInvalidParentError())
         .RuleForValue(duplicate).IsFalse(new LookupDuplicateCodeError());
      return validator.Validate(new LookupUpdateRequest(null, true, []))
         ? Result.Success() : Result.Failure(validator.Messages);
   }

   private static Result ValidateTranslations(IReadOnlyList<LookupTranslationRequest>? translations)
   {
      if (translations == null || translations.Count == 0 || translations.Any(t => t == null) ||
          translations.Select(t => LanguageOptions.Normalize(t.LanguageCode)).Distinct().Count() != translations.Count)
         return Result.Failure(new LookupInvalidTranslationsError());

      foreach (var translation in translations)
      {
         var validator = new FluentValidator<LookupTranslationRequest>()
            .RuleFor(x => x.LanguageCode).IsRequired().MaxLength(SharedConst.Lookup.LanguageMaxLength)
            .RuleFor(x => x.Code).IsRequired().MaxLength(SharedConst.Lookup.CodeMaxLength)
            .RuleFor(x => x.Value).IsRequired().MaxLength(SharedConst.Lookup.ValueMaxLength)
            .RuleForValue(LanguageOptions.IsSupported(translation.LanguageCode))
               .IsTrue(new InvalidLanguageError(translation.LanguageCode));
         if (!validator.Validate(translation)) return Result.Failure(validator.Messages);
      }
      return Result.Success();
   }
}
