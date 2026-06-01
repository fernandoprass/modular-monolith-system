using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;

namespace IAM.Domain.Mappers;

public static class UserMappers
{
   public static UserDto ToUserDto(this User user)
   {
      return new UserDto
      {
         Id = user.Id,
         Name = user.Name,
         Email = user.Email,
         IsActive = user.IsActive,
         IsSystemAdmin = user.IsSystemAdmin,
         IsOrganizationAdmin = user.IsOrganizationAdmin,
         CreatedAt = user.CreatedAt,
         EmailVerifiedAt = user.EmailVerifiedAt,
         LastLoginAt = user.LastLoginAt,
         OrganizationId = user.OrganizationId,
         OrganizationName = user.Organization?.Name ?? string.Empty
      };
   }

   public static UserDto ToUserDto(this UserPasswordDto user)
   {
      return new UserDto
      {
         Id = user.Id,
         Name = user.Name,
         Email = user.Email,
         IsActive = user.IsActive,
         IsSystemAdmin = user.IsSystemAdmin,
         IsOrganizationAdmin = user.IsOrganizationAdmin,
         CreatedAt = user.CreatedAt,
         EmailVerifiedAt = user.EmailVerifiedAt,
         LastLoginAt = user.LastLoginAt,
         OrganizationId = user.OrganizationId,
         OrganizationName = user.OrganizationName
      };
   }

   public static UserPasswordDto ToUserPasswordDto(this User user)
   {
      return new UserPasswordDto
      {
         Id = user.Id,
         Name = user.Name,
         Email = user.Email,
         PasswordHash = user.PasswordHash,
         IsActive = user.IsActive,
         IsSystemAdmin = user.IsSystemAdmin,
         IsOrganizationAdmin = user.IsOrganizationAdmin,
         CreatedAt = user.CreatedAt,
         EmailVerifiedAt = user.EmailVerifiedAt,
         LastLoginAt = user.LastLoginAt,
         LockedOutUntil = user.LockedOutUntil,
         FailedLoginAttempts = user.NumFailedLoginAttempts,
         OrganizationId = user.OrganizationId,
         OrganizationName = user.Organization?.Name ?? string.Empty,
         OrganizationIsActive = user.Organization?.IsActive ?? false,
         UserRoles = user.UserRoles
      };
   }
}
