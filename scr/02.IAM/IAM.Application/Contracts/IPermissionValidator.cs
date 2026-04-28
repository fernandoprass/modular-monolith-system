using IAM.Domain.DTOs.Requests;
using Myce.Response;

namespace IAM.Application.Contracts;

public interface IPermissionValidator
{
   Result ValidateCreate(PermissionCreateRequest request, bool codeAlreadyExists);
   Result ValidateUpdate(PermissionUpdateRequest request, bool codeAlreadyExists, bool permissionExists);
   Result ValidateAssign(RolePermissionAssignRequest request, bool roleExists, bool allPermissionsExist);
}
