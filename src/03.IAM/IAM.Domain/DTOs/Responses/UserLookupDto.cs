namespace IAM.Domain.DTOs.Responses;

public sealed record UserLookupDto(
   Guid Id,
   string Name,
   bool IsActive,
   Guid OrganizationId
);