using IAM.API.Configure;
using IAM.API.Middlewares;
using IAM.Domain;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Middlewares
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString(IamConst.Database.ConnectionString);
builder.Services.AddDbContext<IamDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddSharedInfrastructure(builder.Configuration, connectionString);

DependencyInjection.Configure(builder);

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

static async Task MigrateDatabase(WebApplication app)
{
   if (app.Environment.IsDevelopment())
   {
      using var scope = app.Services.CreateScope();
      var dbIam = scope.ServiceProvider.GetRequiredService<IamDbContext>();
      dbIam.Database.Migrate();
      var dbShared = scope.ServiceProvider.GetRequiredService<SharedDbContext>();
      dbShared.Database.Migrate();
   }
}

