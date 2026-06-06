namespace Core.API.EndToEnd.Tests.Infrastructure;

internal sealed record LoginData
{
   public string Token { get; init; } = string.Empty;
}
