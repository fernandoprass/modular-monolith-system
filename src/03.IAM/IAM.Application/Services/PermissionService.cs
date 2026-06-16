using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Messages;

namespace IAM.Application.Services;

public class PermissionService(
   IIamUnitOfWork iamUnitOfWork,
   IUserContext userContext,
   IPermissionValidator permissionValidator,
   IPermissionQueryRepository permissionQueryRepository,
   IRolePermissionCacheInvalidator rolePermissionAuthorizationCache,
   IIamEventPublisher eventPublisher,
   IEventPublisher? sharedEventPublisher = null) : BaseService(userContext, sharedEventPublisher), IPermissionService
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IPermissionValidator _permissionValidator = permissionValidator;
   private readonly IPermissionQueryRepository _permissionQueryRepository = permissionQueryRepository;
   private readonly IRolePermissionCacheInvalidator _rolePermissionAuthorizationCache = rolePermissionAuthorizationCache;
   private readonly IIamEventPublisher _eventPublisher = eventPublisher;

   public async Task<Result<PermissionDto>> CreateAsync(PermissionCreateRequest request, CancellationToken cancellationToken = default)
   {
      bool codeExists = await GetCodeExistsAsync(request.Module, request.Resource, request.Action, cancellationToken);
      var validation = _permissionValidator.ValidateCreate(request, codeExists);

      if (!validation.IsSuccess)
         return Result<PermissionDto>.Failure(validation.Messages);

      var permission = Permission.Create(request.Module, request.Resource, request.Action, request.Title, request.Description, request.IsActive);

      await _iamUnitOfWork.Permissions.AddAsync(permission, cancellationToken);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      return Result<PermissionDto>.Success(permission.ToPermissionDto());
   }

   public async Task<Result> UpdateAsync(Guid id, PermissionUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var permission = await _iamUnitOfWork.Permissions.GetByIdAsync(id, cancellationToken);
      bool codeExists = await GetCodeExistsAsync(request.Module, request.Resource, request.Action, id, cancellationToken);
      var validation = _permissionValidator.ValidateUpdate(request, codeExists, permissionExists: permission != null);

      if (!validation.IsSuccess)
         return validation;

      permission!.Update(request.Module, request.Resource, request.Action, request.Title, request.Description, request.IsActive);
      _iamUnitOfWork.Permissions.Update(permission);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var permission = await _iamUnitOfWork.Permissions.GetByIdAsync(id, cancellationToken);
      if (permission == null)
         return Result.Failure(new NotFoundError(IamConst.Entity.Permission));

      await _iamUnitOfWork.Permissions.DeleteAsync(id, cancellationToken);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   public async Task<Result<IEnumerable<PermissionDto>>> GetByParams(PermissionSearchRequest request, CancellationToken cancellationToken = default)
   {
      var permissions = await _permissionQueryRepository.GetByParams(request, cancellationToken);
      return Result<IEnumerable<PermissionDto>>.Success(permissions);
   }

   public async Task<Result<IEnumerable<PermissionDto>>> GetByRoleId(Guid roleId, CancellationToken cancellationToken = default)
   {
      var permissions = await _permissionQueryRepository.GetByRoleIdAsync(roleId, cancellationToken);
      return Result<IEnumerable<PermissionDto>>.Success(permissions);
   }

   public async Task<Result<IEnumerable<PermissionDto>>> GetAvailablePermissionByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
   {
      var permissions = await _permissionQueryRepository.GetByAvailablePermissionsRoleIdAsync(roleId, cancellationToken);
      return Result<IEnumerable<PermissionDto>>.Success(permissions);
   }

   public async Task<Result> AssignToRoleAsync(RolePermissionAssignRequest request, CancellationToken cancellationToken = default)
   {
      var role = await _iamUnitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);

      return await ExecuteIfUserOwnsAsync(role?.OrganizationId, async (ct) =>
      {
         var permissionIds = request.PermissionIds?.Distinct().ToList() ?? [];
         var permissionsFound = await _iamUnitOfWork.Permissions.CountByIdsAsync(permissionIds, ct);
         var allPermissionsExist = permissionsFound == permissionIds.Count;

         var validation = _permissionValidator.ValidateAssign(request, role != null, allPermissionsExist);

         if (!validation.IsSuccess) return validation;

         foreach (var permissionId in permissionIds)
         {
            var rolePermission = role!.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);

            if (rolePermission == null)
            {
               role.AddPermission(permissionId);
            }
         }

         _iamUnitOfWork.Roles.Update(role!);
         await _iamUnitOfWork.SaveChangesAsync(ct);
         await _rolePermissionAuthorizationCache.RemoveAsync(role!.Id, ct);

         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Permissions,
            IamConst.Logger.Action.Assign,
            AuditPrivacyLevel.High,
            RetentionPolicy.Extended,
            $"Assigned permissions to role {role.Id}",
            role.Id,
            request,
            ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }

   public async Task<Result> UnassignFromRoleAsync(RolePermissionUnassignRequest request, CancellationToken cancellationToken = default)
   {
      var role = await _iamUnitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);

      return await ExecuteIfUserOwnsAsync(role?.OrganizationId, async (ct) =>
      {
         var permissionIds = request.PermissionIds?.Distinct().ToList() ?? [];
         var rolePermissionIds = new HashSet<Guid>(role?.RolePermissions.Select(rp => rp.PermissionId) ?? []);
         var roleHasAllPermissions = role is not null && permissionIds.All(rolePermissionIds.Contains);

         var validation = _permissionValidator.ValidateUnassign(request, role != null, roleHasAllPermissions);

         if (!validation.IsSuccess) return validation;

         foreach (var permissionId in permissionIds)
         {
            role!.RemovePermission(permissionId);
         }

         _iamUnitOfWork.Roles.Update(role!);
         await _iamUnitOfWork.SaveChangesAsync(ct);
         await _rolePermissionAuthorizationCache.RemoveAsync(role!.Id, ct);

         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Permissions,
            IamConst.Logger.Action.Unassign,
            AuditPrivacyLevel.High,
            RetentionPolicy.Extended,
            $"Unassigned permissions from role {role.Id}",
            role.Id,
            request,
            ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }

   public async Task<Result<PermissionDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
   {
      var permission = await _permissionQueryRepository.GetByCodeAsync(code, cancellationToken);
      if (permission == null)
         return Result<PermissionDto>.Failure(new NotFoundError(IamConst.Entity.Permission));

      return Result<PermissionDto>.Success(permission);
   }

   private async Task<bool> GetCodeExistsAsync(string module, string resource, string action, CancellationToken cancellationToken)
   {
      var code = Permission.BuildCode(module, resource, action);
      return await _permissionQueryRepository.CodeExistsAsync(code, cancellationToken);
   }

   private async Task<bool> GetCodeExistsAsync(string module, string resource, string action, Guid excludedId, CancellationToken cancellationToken)
   {
      var code = Permission.BuildCode(module, resource, action);
      return await _permissionQueryRepository.CodeExistsAsync(code, excludedId, cancellationToken);
   }
}
