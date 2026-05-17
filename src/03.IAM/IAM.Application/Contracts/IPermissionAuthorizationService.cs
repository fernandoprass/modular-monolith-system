using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;

namespace IAM.Application.Contracts;

public interface IPermissionAuthorizationService
{
   Task<PermissionCheckResponse> CheckPermissionAsync(PermissionCheckRequest request, CancellationToken cancellationToken = default);
}
