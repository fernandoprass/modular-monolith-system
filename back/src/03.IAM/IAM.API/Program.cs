using IAM.API.Configure;
using IAM.API.Middlewares;
using IAM.Domain;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
   options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); 

builder.Services.AddEndpointsApiExplorer();

// Add Middlewares
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddIamModule(builder.Configuration);

ApiVersioning.Configure(builder);

JWTAuthentication.Configure(builder);

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("api/v{version:apiVersion}/iam/health", () => Results.Ok(new { Status = "Ok", Module = IamConst.System.ModuleName }));

app.Run();
