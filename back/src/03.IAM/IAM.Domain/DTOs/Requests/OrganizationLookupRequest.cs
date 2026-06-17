public sealed record OrganizationLookupRequest(
   Guid? Id,
   string? Search,
   bool IncludeInactive = false,
   int Take = 25
);