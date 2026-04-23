using IAM.Domain.DTOs.Requests;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IOrganizationValidator
{
   Result ValidateCreate(OrganizationCreateRequest request, bool codeExists);
   Result ValidateUpdate(OrganizationUpdateRequest request, bool organizationExists);
   Result ValidateUpdateCode(OrganizationUpdateCodeRequest request, bool newCodeExists);
}
