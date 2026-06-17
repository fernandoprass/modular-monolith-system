namespace IAM.Domain.DTOs.Requests;

public sealed record UserLookupRequest(
   Guid? Id,
   string? Search,
   bool IncludeInactive = false,
   int Take = 25
);