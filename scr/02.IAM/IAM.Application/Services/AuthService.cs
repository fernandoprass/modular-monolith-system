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
using Myce.Response.Messages;
using Shared.Application.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IAM.Application.Services;

public class AuthService(
   IUserQueryRepository userQueryRepository,
   IUserService userService,
   IParameterService parameterService,
   IConfiguration configuration) : IAuthService
{
   private readonly IUserQueryRepository _userQueryRepository = userQueryRepository;
   private readonly IUserService _userService = userService;
   private readonly IParameterService _parameterService = parameterService;
   private readonly string _jwtSecret = configuration["Jwt:Secret"] ?? "your-super-secret-jwt-key-here-make-it-long-and-secure";

   public async Task<Result<LoginResponse?>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
   {
      var user = await _userQueryRepository.GetByEmailWithPasswordAsync(request.Email, cancellationToken);

      var result = await Validate(user, request.Password, cancellationToken);

      if (!result.IsSuccess) return Result<LoginResponse?>.Failure(result.Messages);

      await _userService.UpdateLastLoginAsync(user.Id, cancellationToken);

      var userDto = user.ToUserDto();
      var expiresAt = await GetJwtExpireTime();
      var token = GenerateJwtToken(userDto, expiresAt);

      var response = new LoginResponse(token, expiresAt, userDto);

      return Result<LoginResponse?>.Success(response);
   }

   private async Task<Result> Validate(UserPasswordDto? user, string password, CancellationToken cancellationToken)
   {
      var passwordHash = user?.PasswordHash ?? GetDummyHash();
      var isPasswordCorrect = Argon2.Verify(passwordHash, password);

      if (user is null || !isPasswordCorrect)
      {
         if (user.LockedOutUntil.HasValue && DateTime.UtcNow < user.LockedOutUntil.Value)
         {
            int minutesRemaining = (int)((user?.LockedOutUntil - DateTime.UtcNow)?.TotalMinutes ?? 0);
            return Result.Failure(new AccountLockedError(minutesRemaining));
         }

         int maxFailedAttempts = await _parameterService.GetIntAsync(IamParam.Security.MaxFailedLoginAttempts, cancellationToken);
         int remainingAttempts = maxFailedAttempts - 1 - (user?.FailedLoginAttempts ?? 0);

         if (user is not null && remainingAttempts <= 0)
         {
            await _userService.UpdateFailedLoginAsync(user.Id);
         }

         return Result.Failure(new UnauthorizedError(remainingAttempts));
      }

      if (!user.IsActive || !user.OrganizationIsActive)
      {
         return Result.Failure(new UnauthorizedError());
      }

      return Result.Success();
   }

   private static string GetDummyHash()
   {
      // Use the dummy hash for timing attack prevention even if the user is not found
      return "$argon2id$v=19$m=65536,t=2,p=1$"
          + Convert.ToBase64String(Encoding.UTF8.GetBytes("fake-salt"))
          + "$" + Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
   }

   private string GenerateJwtToken(UserDto user, DateTime expiresAt)
   {
      var claims = new[]
          {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(IamConst.Security.Claim.IsSystemAdmin, user.IsSystemAdmin.ToString()),
            new Claim(IamConst.Security.Claim.UserOwnerId, user.OrganizationId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: IamConst.Security.Claim.Issuer,
          audience: IamConst.Security.Claim.Audience,
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
}