using IAM.Domain.Enums;

namespace IAM.Domain.DTOs.Requests;

public sealed record OrganizationSearchRequest(
   OrganizationType? Type = null,
   string? Code = null,
   string? Name = null,
   Guid? OrganizationId = null,
   bool? IsActive = null,
   int PageNumber = 1,
   int PageSize = 25);
