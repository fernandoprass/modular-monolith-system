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
using Shared.Domain.Messages;

namespace IAM.Application.Services;

public class RoleService(
   IIamUnitOfWork iamUnitOfWork,
   IUserContext userContext,
   IRoleValidator roleValidator,
   IRoleQueryRepository roleQueryRepository) : BaseService(userContext), IRoleService
{
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IRoleValidator _roleValidator = roleValidator;
   private readonly IRoleQueryRepository _roleQueryRepository = roleQueryRepository;

   public async Task<Result<RoleDto>> CreateAsync(RoleCreateRequest request, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(request.OrganizationId, async (ct) =>
      {
         var nameExists = await _roleQueryRepository.NameExistsAsync(request.Name, request.OrganizationId, ct);
         var validation = _roleValidator.ValidateCreate(request, nameExists);

         if (!validation.IsSuccess)
            return Result<RoleDto>.Failure(validation.Messages);

         var role = Role.Create(request.Name, request.Description, request.IsDefault, request.IsActive, request.OrganizationId);

         await _iamUnitOfWork.Roles.AddAsync(role, ct);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         return Result<RoleDto>.Success(role.ToRoleDto());
      }, cancellationToken);
   }

   public async Task<Result> UpdateAsync(Guid id, RoleUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var role = await _iamUnitOfWork.Roles.GetByIdAsync(id, cancellationToken);

      if (role == null)
         return Result.Failure(new NotFoundError(IamConst.Entity.Role));

      return await ExecuteIfUserOwnsAsync(role.OrganizationId, async (ct) =>
      {
         var validation = _roleValidator.ValidateUpdate(id, request, role.IsDefault);

         if (!validation.IsSuccess)
            return validation;

         role.Update(request.Name, request.Description, request.IsDefault, request.IsActive);
         _iamUnitOfWork.Roles.Update(role);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         return Result.Success();
      }, cancellationToken);
   }

   public async Task<Result> AssignToUserAsync(RoleAssignRequest request, CancellationToken cancellationToken = default)
   {
      var user = await _iamUnitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

      if (user == null)
         return Result.Failure(new NotFoundError(IamConst.Entity.User));

      return await ExecuteIfUserOwnsAsync(user.OrganizationId, async (ct) =>
      {
         // Simple check for all roles existing and being within same organization or global
         //todo fix it
        // var roles = await _roleQueryRepository.NameExistsAsync(_userContext.UserOwnerId, ct);
         var allRequestedRolesExist = true;//request.Roles.All(roleAssigned => roles.Any(r => r.Id == roleAssigned.Id && r.IsActive));

         var validation = _roleValidator.ValidateAssign(request, true, allRequestedRolesExist);

         if (!validation.IsSuccess)
            return validation;

         user.ClearRoles();
         foreach (var role in request.Roles)
         {
            user.AddRole(role.Id, role.ExpiresAt);
         }

         _iamUnitOfWork.Users.Update(user);
         await _iamUnitOfWork.SaveChangesAsync(ct);

         return Result.Success();
      }, cancellationToken);
   }

   public async Task<Result<IEnumerable<RoleDto>>> GetAllAsync(string name, CancellationToken cancellationToken = default)
   {
      var roles = await _roleQueryRepository.GetAllAsync(name, _userContext.UserOwnerId, cancellationToken);

      return Result<IEnumerable<RoleDto>>.Success(roles);
   }

   public async Task<Result<IEnumerable<PermissionDto>>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
   {
      var user = await _iamUnitOfWork.Users.GetByIdAsync(userId, cancellationToken);
      if (user == null)
         return Result<IEnumerable<PermissionDto>>.Failure(new NotFoundError(IamConst.Entity.User));

      return await ExecuteIfUserOwnsAsync(user.OrganizationId, async (ct) =>
      {
         var permissions = await _roleQueryRepository.GetUserPermissionsAsync(userId, ct);
         var permissionDto = permissions.Select(p => p.ToPermissionDto());
         return Result<IEnumerable<PermissionDto>>.Success(permissionDto);
      }, cancellationToken);
   }
}
