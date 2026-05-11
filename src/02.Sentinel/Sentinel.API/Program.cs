using Sentinel.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSentinelInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Module = "Sentinel" }));

app.Run();

public partial class Program { }
