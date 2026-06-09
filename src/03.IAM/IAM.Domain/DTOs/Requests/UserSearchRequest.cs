namespace IAM.Domain.DTOs.Requests;

public sealed record UserSearchRequest(
   string? Name,
   string? Email,
   Guid? OrganizationId,
   int PageNumber = 1,
   int PageSize = 25
);