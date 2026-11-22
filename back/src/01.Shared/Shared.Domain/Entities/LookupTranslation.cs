using Shared.Domain.DTOs.Requests;

namespace Shared.Domain.Entities;

internal class LookupTranslation : EntityAudited
{
   public string LanguageCode { get; private set; } = string.Empty;
   public string Code { get; private set; } = string.Empty;
   public string Value { get; private set; } = string.Empty;
   public string? Description { get; private set; }
   public bool IsActive { get; private set; }
   public Guid LookupId { get; private set; }
   public Lookup Lookup { get; private set; } = null!;

   private LookupTranslation() { }

   public static LookupTranslation Create(Guid lookupId, LookupTranslationRequest request)
   {
      var translation = new LookupTranslation
      {
         Id = Guid.CreateVersion7(), LookupId = lookupId,
         LanguageCode = LanguageOptions.Normalize(request.LanguageCode)
      };
      translation.Update(request);
      return translation;
   }

   public void Update(LookupTranslationRequest request)
   {
      Code = request.Code.Trim();
      Value = request.Value.Trim();
      Description = request.Description?.Trim();
      IsActive = request.IsActive;
   }
}
