using Courier.API.Configure;
using Courier.API.Middlewares;
using Courier.Domain;
using Courier.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Infrastructure;
using Shared.Infrastructure.Security;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCourierInfrastructure(builder.Configuration);
var sharedConnectionString = builder.Configuration.GetConnectionString(SharedConst.Database.ConnectionString)
   ?? throw new InvalidOperationException("Shared database connection string is required.");
builder.Services.AddSharedInfrastructure(builder.Configuration, sharedConnectionString);
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, AspNetUserContext>();

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

app.MapGet("api/v{version:apiVersion}/courier/health", async (
   CourierDbContext courierDbContext,
   IConnectionMultiplexer redis,
   CancellationToken cancellationToken) =>
{
   await courierDbContext.PingAsync(cancellationToken);
   await redis.GetDatabase().PingAsync();

   return Results.Ok(new { Status = "Ok", Module = CourierConst.System.ModuleName });
});

app.Run();

public partial class Program { }
