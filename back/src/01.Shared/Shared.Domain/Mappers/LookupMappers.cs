using Shared.Domain.DTOs.Responses;
using Shared.Domain.Entities;

namespace Shared.Domain.Mappers;

internal static class LookupMappers
{
   public static LookupDto ToLookupDto(this Lookup lookup) => new(
      lookup.Id, lookup.Type, lookup.Code, lookup.IsActive, lookup.ParentId, lookup.OrganizationId,
      lookup.Translations.OrderBy(t => t.LanguageCode).Select(t => new LookupTranslationDto(
         t.Id, t.LanguageCode, t.Code, t.Value, t.Description, t.IsActive, t.LookupId)).ToArray());
}
