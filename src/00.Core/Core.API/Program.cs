using Core.API;
using Core.API.Configure;
using Core.API.Middlewares;
using Courier.API.Configure;
using Courier.API.Controllers;
using IAM.API.Configure;
using IAM.API.Controllers;
using Sentinel.API.Configure;
using Sentinel.API.Controllers;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services
   .AddControllers()
   .AddApplicationPart(typeof(UserController).Assembly)
   .AddApplicationPart(typeof(LogController).Assembly)
   .AddApplicationPart(typeof(EmailController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
   options.AddPolicy(FrontendCorsPolicy, policy =>
   {
      var allowedOrigins = builder.Configuration
         .GetSection("Cors:AllowedOrigins")
         .Get<string[]>() ?? [];

      policy
         .WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod();
   });
});

builder.Services.AddIamModule(builder.Configuration);
builder.Services.AddSentinelModule(builder.Configuration);
builder.Services.AddCourierModule(builder.Configuration);

Core.API.Configure.ApiVersioning.Configure(builder);
JwtAuthentication.Configure(builder);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
