using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shared.Domain;
using System.Text;

namespace IAM.API.Configure
{
   public static class JWTAuthentication
   {
      public static void Configure(WebApplicationBuilder builder)
      {
         var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-jwt-key-here-make-it-long-and-secure";
         var key = Encoding.UTF8.GetBytes(jwtSecret);

         builder.Services.AddAuthorization();

         builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = SharedConst.Security.Claim.Issuer,
                   ValidAudience = SharedConst.Security.Claim.Audience,
                   IssuerSigningKey = new SymmetricSecurityKey(key),
                   NameClaimType = JwtRegisteredClaimNames.Sub,
                   RoleClaimType = SharedConst.Security.Claim.Role
                };
             });
      }
   }
}
