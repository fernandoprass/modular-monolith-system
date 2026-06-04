using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Core.API.EndToEnd.Tests.Infrastructure;

public sealed class AuthenticatedCoreApiClient
{
   private readonly HttpClient _httpClient;

   public AuthenticatedCoreApiClient(HttpClient httpClient, string token)
   {
      _httpClient = httpClient;
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
   }

   public async Task<OrganizationDto> GetOrganizationAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var response = await _httpClient.GetAsync($"/api/v1/iam/organizations/{id}", cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<OrganizationDto>(cancellationToken);
   }

   public async Task UpdateOrganizationAsync(
      Guid id,
      OrganizationUpdateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await _httpClient.PutAsJsonAsync($"/api/v1/iam/organizations/{id}", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task UpdateOrganizationCodeAsync(
      Guid id,
      OrganizationUpdateCodeRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await _httpClient.PatchAsJsonAsync($"/api/v1/iam/organizations/{id}/code", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task DeleteOrganizationAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var response = await _httpClient.DeleteAsync($"/api/v1/iam/organizations/{id}", cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }
}
