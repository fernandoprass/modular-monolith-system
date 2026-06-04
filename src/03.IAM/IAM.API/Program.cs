using IAM.API.Configure;
using IAM.API.Middlewares;
using IAM.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Middlewares
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddIamModule(builder.Configuration);

ApiVersioning.Configure(builder);

JWTAuthentication.Configure(builder);

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("api/v{version:apiVersion}/iam/health", () => Results.Ok(new { Status = "Ok", Module = IamConst.System.ModuleName }));

app.Run();
