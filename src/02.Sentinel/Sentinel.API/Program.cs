using Sentinel.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Infrastructure.Authorization;
using Shared.Infrastructure.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSentinelInfrastructure(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IRolePermissionAuthorizationCache, RolePermissionAuthorizationCache>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, AspNetUserContext>();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-jwt-key-here-make-it-long-and-secure";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
      options.TokenValidationParameters = new TokenValidationParameters
      {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = SharedConst.Security.Claim.Issuer,
         ValidAudience = SharedConst.Security.Claim.Audience,
         IssuerSigningKey = new SymmetricSecurityKey(key)
      };
   });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Module = "Sentinel" }));

app.Run();

public partial class Program { }
