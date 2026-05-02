using IAM.Domain.DTOs.Requests;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IRoleValidator
{
   Result ValidateCreate(RoleCreateRequest request, bool nameAlreadyExists);
   Result ValidateUpdate(RoleUpdateRequest request, bool roleExists);
   Result ValidateAssign(RoleAssignRequest request, bool userExists, bool allRolesAvailable);
   Result ValidateUnassign(RoleUnassignRequest request, bool userExists, bool allRolesAreAssigned);
}
