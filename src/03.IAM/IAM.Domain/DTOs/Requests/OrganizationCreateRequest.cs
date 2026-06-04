using IAM.Domain.Enums;

namespace IAM.Domain.DTOs.Requests;

public sealed record OrganizationCreateRequest
(
    OrganizationType Type,
    string Name,
    string Code,
    string Description,
    string DefaultLanguage,
    OrganizationUserCreateRequest User
);