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
using Shared.Domain.Messages;

namespace IAM.Application.Services;

public class RoleService(
   IIamUnitOfWork iamUnitOfWork,
   IUserContext userContext,
   IRoleValidator roleValidator,
   IRoleQueryRepository roleQueryRepository,
   IIamEventPublisher eventPublisher) : BaseService(userContext), IRoleService
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IRoleValidator _roleValidator = roleValidator;
   private readonly IRoleQueryRepository _roleQueryRepository = roleQueryRepository;
   private readonly IIamEventPublisher _eventPublisher = eventPublisher;

   public async Task<Result<RoleDto>> CreateAsync(RoleCreateRequest request, CancellationToken cancellationToken = default)
   {
      var nameExists = await _roleQueryRepository.NameExistsAsync(request.Name, request.OrganizationId, cancellationToken);
      var validation = _roleValidator.ValidateCreate(request, nameExists);

      if (!validation.IsSuccess)
         return Result<RoleDto>.Failure(validation.Messages);

      var role = Role.Create(request.Name, request.Description, request.IsDefault, request.IsActive, request.OrganizationId);

      await _iamUnitOfWork.Roles.AddAsync(role, cancellationToken);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      await _eventPublisher.NotifyAuditLogAsync(
         IamConst.Logger.Feature.Roles,
         IamConst.Logger.Action.Create,
         AuditPrivacyLevel.Medium,
         $"Created role {role.Name}",
         role.Id,
         request,
         cancellationToken);

      return Result<RoleDto>.Success(role.ToRoleDto());
   }

   public async Task<Result> UpdateAsync(Guid id, RoleUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var role = await _iamUnitOfWork.Roles.GetByIdAsync(id, cancellationToken);

      return await ExecuteIfUserOwnsAsync(role?.OrganizationId, async (ct) =>
      {
         var validation = _roleValidator.ValidateUpdate(request, role != null);

         if (!validation.IsSuccess)
            return validation;

         role.Update(request.Name, request.Description, request.IsDefault, request.IsActive);

         _iamUnitOfWork.Roles.Update(role);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Roles,
            IamConst.Logger.Action.Update,
            AuditPrivacyLevel.Medium,
            $"Updated role {role.Id}",
            role.Id,
            request,
            ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }

   public async Task<Result> AssignToUserAsync(RoleAssignRequest request, CancellationToken cancellationToken = default)
   {
      var user = await _iamUnitOfWork.Users.GetByIdWithRolesAsync(request.UserId, cancellationToken);

      return await ExecuteIfUserOwnsAsync(user?.OrganizationId, async (ct) =>
      {
         bool allRequestedRolesAvailable = user == null ? true : await ValidateRolesAvailability(request, user.OrganizationId, cancellationToken);

         var validation = _roleValidator.ValidateAssign(request, user is not null, allRequestedRolesAvailable);

         if (!validation.IsSuccess) return validation;

         foreach (var role in request.Roles)
         {
            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.RoleId);

            if (userRole != null)
            {
               userRole.UpdateExpiration(role.ExpiresAt);
            }
            else
            {
               user.AddRole(role.RoleId, role.ExpiresAt);
            }
         }

         _iamUnitOfWork.Users.Update(user);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Roles,
            IamConst.Logger.Action.Assign,
            AuditPrivacyLevel.High,
            $"Assigned roles to user {user.Id}",
            user.Id,
            request,
            ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }

   /// <summary>
   /// Verifies if requested roles are available: exists in DB, is active, and matches organization scope or system admin bypass
   /// </summary>
   private async Task<bool> ValidateRolesAvailability(RoleAssignRequest request, Guid organizationId, CancellationToken cancellationToken)
   {
      var requestedRoleIds = request.Roles.Select(r => r.RoleId).Distinct().ToList();
      var numberOfRolesToAssign = await _roleQueryRepository.CountRolesByRoleIdsAsync(requestedRoleIds, organizationId, cancellationToken);

      var allRequestedRolesExist = requestedRoleIds.Count == numberOfRolesToAssign;
      return allRequestedRolesExist;
   }

   public async Task<Result> UnassignFromUserAsync(RoleUnassignRequest request, CancellationToken cancellationToken = default)
   {
      var user = await _iamUnitOfWork.Users.GetByIdWithRolesAsync(request.UserId, cancellationToken);

      return await ExecuteIfUserOwnsAsync(user?.OrganizationId, async (ct) =>
      {
         var userRoleIds = new HashSet<Guid>(user?.UserRoles.Select(ur => ur.RoleId));

         bool userHasAllRoles = user != null && request.RoleIds.All(userRoleIds.Contains);

         var validation = _roleValidator.ValidateUnassign(request, user is not null, userHasAllRoles);

         if (!validation.IsSuccess) return validation;

         var rolesToUnassign = request.RoleIds.Distinct();
         foreach (var roleId in rolesToUnassign)
         {
            user.RemoveRole(roleId);
         }

         _iamUnitOfWork.Users.Update(user);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Roles,
            IamConst.Logger.Action.Unassign,
            AuditPrivacyLevel.High,
            $"Unassigned roles from user {user.Id}",
            user.Id,
            request,
            ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }

   public async Task<Result<IEnumerable<RoleDto>>> GetAsync(RoleSearchRequest request, CancellationToken cancellationToken = default)
   {
      var roles = await _roleQueryRepository.GetAsync(request, cancellationToken);
      return Result<IEnumerable<RoleDto>>.Success(roles);
   }

   public async Task<Result<IEnumerable<PermissionDto>>> GetRolePermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      var user = await _iamUnitOfWork.Users.GetByIdAsync(userId, cancellationToken);
      
      if (user == null) return Result<IEnumerable<PermissionDto>>.Failure(new NotFoundError(IamConst.Entity.User));

      return await ExecuteIfUserOwnsAsync(user.OrganizationId, async (ct) =>
      {
         var permissions = await _roleQueryRepository.GetRolePermissionsByUserIdAsync(userId, ct);
         var permissionDto = permissions.Select(p => p.ToPermissionDto());
         return Result<IEnumerable<PermissionDto>>.Success(permissionDto);
      }, cancellationToken);
   }

   public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
   {
      return await _roleQueryRepository.GetPermissionsByRoleIdAsync(roleId, cancellationToken);
   }

   public async Task<IEnumerable<Guid>> GetDefaultRolesByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
   {
      return await _roleQueryRepository.GetDefaultRolesByOrganizationIdAsync(organizationId, cancellationToken);
   }
}
