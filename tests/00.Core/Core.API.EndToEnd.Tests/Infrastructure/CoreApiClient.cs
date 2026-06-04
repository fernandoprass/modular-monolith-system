using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using System.Net.Http.Json;

namespace Core.API.EndToEnd.Tests.Infrastructure;

public sealed class CoreApiClient(HttpClient httpClient)
{
   private readonly HttpClient _httpClient = httpClient;

   public async Task<OrganizationDto> CreateOrganizationAsync(
      OrganizationCreateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await _httpClient.PostAsJsonAsync("/api/v1/iam/organizations", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<OrganizationDto>(cancellationToken);
   }

   public async Task<AuthenticatedCoreApiClient> LoginAsync(
      string email,
      string password,
      CancellationToken cancellationToken = default)
   {
      var response = await _httpClient.PostAsJsonAsync(
         "/api/v1/iam/users/login",
         new UserLoginRequest(email, password),
         cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      var login = await response.ReadResultDataAsync<LoginData>(cancellationToken);
      if (string.IsNullOrWhiteSpace(login.Token))
      {
         throw new InvalidOperationException("Login response did not contain a token.");
      }

      return new AuthenticatedCoreApiClient(_httpClient, login.Token);
   }
}
