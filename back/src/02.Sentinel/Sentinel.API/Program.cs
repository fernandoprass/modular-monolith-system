using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sentinel.API.Configure;
using Sentinel.API.Middlewares;
using Sentinel.Domain;
using Shared.Domain;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
   options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSentinelModule(builder.Configuration);
builder.Services.AddAuthorization();

ApiVersioning.Configure(builder);

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

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
   app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("api/v{version:apiVersion}/sentinel/health", () => Results.Ok(new { Status = "Ok", Module = SentinelConst.System.ModuleName }));

app.Run();

public partial class Program { }
