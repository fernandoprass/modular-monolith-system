using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Mappers;
using IAM.Domain.Messages;
using IAM.Domain.QueryRepositories;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SharedPermissionService = Shared.Application.Contracts.IRolePermissionCache;

namespace IAM.Application.Services;

public class AuthService(
   IRoleQueryRepository roleQueryRepository,
   IUserService userService,
   IParameterService parameterService,
   SharedPermissionService permissionService,
   IIamEventPublisher eventPublisher,
   IConfiguration configuration) : IAuthService
{
   private readonly IRoleQueryRepository _roleQueryRepository = roleQueryRepository;
   private readonly IUserService _userService = userService;
   private readonly IParameterService _parameterService = parameterService;
   private readonly SharedPermissionService _permissionService = permissionService;
   private readonly IIamEventPublisher _eventPublisher = eventPublisher;
   private readonly string _jwtSecret = GetJwtSecret(configuration);

   public async Task<Result<LoginResponse?>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
   {
      var normalizedEmail = request.Email.ToLowerInvariant().Trim();
      var user = await _userService.GetByEmailWithPasswordAsync(normalizedEmail, cancellationToken);

      var result = await Validate(user, request.Password, cancellationToken);

      if (!result.IsSuccess)
      {
         await PublishLoginAuditLogAsync(false, normalizedEmail, user, result, cancellationToken);
         return Result<LoginResponse?>.Failure(result.Messages);
      }

      await _userService.UpdateLastLoginAsync(user!.Id, cancellationToken);

      var expiresAt = await GetJwtExpireTime();
      await HydratePermissionCacheAsync(user.RoleIds, expiresAt, cancellationToken);
      var token = GenerateJwtToken(user, expiresAt);

      var response = new LoginResponse(token, expiresAt, user.ToUserDto());

      await PublishLoginAuditLogAsync(true, normalizedEmail, user, result, cancellationToken);

      return Result<LoginResponse?>.Success(response);
   }

   private async Task PublishLoginAuditLogAsync(
      bool isSuccess,
      string email,
      UserPasswordDto? user,
      Result result,
      CancellationToken cancellationToken)
   {
      var action = isSuccess ? IamConst.Logger.Action.LoginSuccess : IamConst.Logger.Action.LoginFail;
      var description = isSuccess ? $"Successful login for {email}" : $"Failed login for {email}";
      var metadata = new
      {
         Email = email,
         IsSuccess = isSuccess,
         Reasons = result.Messages.Select(message => message.GetType().Name)
      };

      await _eventPublisher.NotifyAuditLogAsync(
         IamConst.Logger.Feature.Authentication,
         action,
         AuditPrivacyLevel.Medium,
         RetentionPolicy.Extended,
         description,
         user?.Id,
         metadata,
         cancellationToken);
   }

   private async Task HydratePermissionCacheAsync(
      IEnumerable<Guid> roleIds,
      DateTime expiresAt,
      CancellationToken cancellationToken)
   {
      var rolePermissionCodes = await _roleQueryRepository.GetPermissionCodesByRoleIdsAsync(roleIds, cancellationToken);
      foreach (var roleId in roleIds)
      {
         var permissionCodes = rolePermissionCodes.Where(rpc => rpc.RoleId == roleId).Select(rpc => rpc.Code);
         await _permissionService.SetPermissionsAsync(
            roleId.ToString(),
            permissionCodes,
            expiresAt,
            cancellationToken);
      }
   }

   private async Task<Result> Validate(UserPasswordDto? user, string password, CancellationToken cancellationToken)
   {
      // 1. Check lockout FIRST (before password verification)
      if (user?.LockedOutUntil.HasValue == true && DateTime.UtcNow < user.LockedOutUntil.Value)
      {
         int minutesRemaining = (int)Math.Ceiling((user.LockedOutUntil.Value - DateTime.UtcNow).TotalMinutes);
         return Result.Failure(new AccountLockedError(minutesRemaining));
      }

      var passwordHash = user?.PasswordHash ?? GetDummyHash();
      var isPasswordCorrect = Argon2.Verify(passwordHash, password);

      // 2. Then verify password
      if (user == null || !isPasswordCorrect)
      {
         if (user != null)
         {
            await _userService.UpdateFailedLoginAsync(user.Id, cancellationToken);
         }

         return Result.Failure(new InvalidEmailPasswordError());
      }

      //3. Finally, check if user and organization are active
      if (!user.IsActive || !user.OrganizationIsActive)
      {
         return Result.Failure(new UnauthorizedAccessError());
      }

      return Result.Success();
   }

   private static string GetDummyHash()
   {
      // Use the dummy hash for timing attack prevention even if the user is not found
      return "$argon2id$v=19$m=65536,t=2,p=1$"
          + Convert.ToBase64String(Encoding.UTF8.GetBytes("fake-salt"))
          + "$" + Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.CreateVersion7().ToString()));
   }

   private string GenerateJwtToken(UserPasswordDto user, DateTime expiresAt)
   {
      var claims = new List<Claim>
          {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email),
            new (JwtRegisteredClaimNames.Name, user.Name),
            new (SharedConst.Security.Claim.Language, user.Language),
            new (SharedConst.Security.Claim.IsSystemAdmin, user.IsSystemAdmin.ToString()),
            new (SharedConst.Security.Claim.IsSupportUser, user.IsSupportUser.ToString()),
            new (SharedConst.Security.Claim.IsOrganizationAdmin, user.IsOrganizationAdmin.ToString()),
            new (SharedConst.Security.Claim.OrganizationId, user.OrganizationId.ToString()),
            new (JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

      foreach (var roleId in user.RoleIds)
      {
         claims.Add(new Claim(SharedConst.Security.Claim.Role, roleId.ToString()));
      }

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: SharedConst.Security.Claim.Issuer,
          audience: SharedConst.Security.Claim.Audience,
          claims: claims,
          expires: expiresAt,
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
   }

   private async Task<DateTime> GetJwtExpireTime()
   {
      int _jwtExpirationHours = await _parameterService.GetIntAsync(IamParam.Security.JwtExpirationInHours);
      var expiresAt = DateTime.UtcNow.AddHours(_jwtExpirationHours);
      return expiresAt;
   }

   private static string GetJwtSecret(IConfiguration configuration)
   {
      var jwtSecret = configuration["Jwt:Secret"];
      if (string.IsNullOrWhiteSpace(jwtSecret))
      {
         throw new InvalidOperationException("JWT secret is required.");
      }

      return jwtSecret;
   }
}
