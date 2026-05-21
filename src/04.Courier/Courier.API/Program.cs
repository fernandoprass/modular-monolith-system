using Courier.API.Configure;
using Courier.Domain;
using Courier.Infrastructure;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddCourierInfrastructure(builder.Configuration);

ApiVersioning.Configure(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
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
