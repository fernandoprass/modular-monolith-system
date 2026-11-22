using Shared.Domain.DTOs.Requests;

namespace Shared.Domain.Entities;

internal class Lookup : EntityAudited
{
   private readonly List<LookupTranslation> _translations = [];
   private readonly List<Lookup> _children = [];

   public string Type { get; private set; } = string.Empty;
   public string Code { get; private set; } = string.Empty;
   public bool IsActive { get; private set; }
   public Guid? ParentId { get; private set; }
   public Guid? OrganizationId { get; private set; }
   public Lookup? Parent { get; private set; }
   public IReadOnlyCollection<Lookup> Children => _children.AsReadOnly();
   public IReadOnlyCollection<LookupTranslation> Translations => _translations.AsReadOnly();

   private Lookup() { }

   // Requests are validated before reaching domain mutation methods.
   public static Lookup Create(LookupCreateRequest request)
   {
      var lookup = new Lookup
      {
         Id = Guid.CreateVersion7(),
         Type = request.Type.Trim().ToLowerInvariant(),
         OrganizationId = request.OrganizationId,
         Code = request.Translations[0].Code.Trim()
      };
      lookup.Update(new(request.ParentId, request.IsActive, request.Translations));
      return lookup;
   }

   public void Update(LookupUpdateRequest request)
   {
      ParentId = request.ParentId;
      IsActive = request.IsActive;
      var languages = request.Translations.Select(t => LanguageOptions.Normalize(t.LanguageCode)).ToHashSet();
      _translations.RemoveAll(t => !languages.Contains(t.LanguageCode));
      foreach (var item in request.Translations)
      {
         var language = LanguageOptions.Normalize(item.LanguageCode);
         var existing = _translations.SingleOrDefault(t => t.LanguageCode == language);
         if (existing == null)
            _translations.Add(LookupTranslation.Create(Id, item));
         else
            existing.Update(item);
      }
   }
}
