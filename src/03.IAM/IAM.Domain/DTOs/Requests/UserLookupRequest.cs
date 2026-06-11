public sealed record UserLookupRequest(
   Guid? Id,
   string? Search,
   Guid? OrganizationId = null,
   bool IncludeInactive = false,
   int Take = 25
);