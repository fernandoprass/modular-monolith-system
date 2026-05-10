using IAM.Domain.Enums;

namespace IAM.Domain.DTOs.Responses;

public sealed record OrganizationDto
(
   Guid Id,
   OrganizationType Type,
   string Code,
   string Name,
   string? Description,
   bool IsActive
);