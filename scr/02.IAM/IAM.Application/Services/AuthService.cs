using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Mappers;
using IAM.Domain.Messages.Errors;
using IAM.Domain.QueryRepositories;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Myce.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IAM.Application.Services;

public interface IAuthService
{
   Task<Result<LoginResponse?>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default);
}

public class AuthService(IUserQueryRepository userQueryRepository,
   IUserService userService,
   IConfiguration configuration) : IAuthService
{
   private readonly IUserQueryRepository _userQueryRepository = userQueryRepository;
   private readonly IUserService _userService = userService;
   private readonly string _jwtSecret = configuration["Jwt:Secret"] ?? "your-super-secret-jwt-key-here-make-it-long-and-secure";
   private readonly int _jwtExpirationHours = int.Parse(configuration["Jwt:ExpirationHours"] ?? "24");

   public async Task<Result<LoginResponse?>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
   {
      var user = await _userQueryRepository.GetByEmailWithPasswordAsync(request.Email, cancellationToken);

      var dummyHash = "$argon2id$v=19$m=65536,t=2,p=1$"
                      + Convert.ToBase64String(Encoding.UTF8.GetBytes("fake-salt"))
                      + "$" + Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

      // Use the dummy hash for timing attack prevention even if the user is not found
      var passwordHash = user?.PasswordHash ?? dummyHash;
      var isPasswordCorrect = Argon2.Verify(passwordHash, request.Password);

      if (user is null || !user.IsActive || !user.CustomerIsActive || !isPasswordCorrect)
      {
         return Result<LoginResponse?>.Failure(new UnauthorizedError());
      }

      var userDto = user.ToUserDto();
      var (token, expiresAt) = GenerateJwtToken(userDto);

      await _userService.UpdateLastLoginAsync(user.Id, cancellationToken);

      var response = new LoginResponse(token, expiresAt, userDto);

      return Result<LoginResponse?>.Success(response);
   }

   private (string, DateTime) GenerateJwtToken(UserDto user)
   {
      var claims = new[]
      {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(IamConst.Security.Claim.IsSystemAdmin, user.IsSystemAdmin.ToString()),
            new Claim(IamConst.Security.Claim.UserOwnerId, user.CustomerId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      var expiresAt = DateTime.UtcNow.AddHours(_jwtExpirationHours);

      var token = new JwtSecurityToken(
          issuer: IamConst.Security.Claim.Issuer,
          audience: IamConst.Security.Claim.Audience,
          claims: claims,
          expires: expiresAt,
          signingCredentials: creds
      );

      return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
   }
}