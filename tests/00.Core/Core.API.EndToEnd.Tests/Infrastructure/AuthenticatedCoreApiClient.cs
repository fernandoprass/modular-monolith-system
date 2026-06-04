using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Core.API.EndToEnd.Tests.Infrastructure;

public sealed class AuthenticatedCoreApiClient
{
   private readonly HttpClient _httpClient;
   private readonly string _token;

   public AuthenticatedCoreApiClient(HttpClient httpClient, string token)
   {
      _httpClient = httpClient;
      _token = token;
   }

   public async Task<OrganizationDto> GetOrganizationAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Get, $"/api/v1/iam/organizations/{id}", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<OrganizationDto>(cancellationToken);
   }

   public async Task UpdateOrganizationAsync(
      Guid id,
      OrganizationUpdateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Put, $"/api/v1/iam/organizations/{id}", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task UpdateOrganizationCodeAsync(
      Guid id,
      OrganizationUpdateCodeRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Patch, $"/api/v1/iam/organizations/{id}/code", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task DeleteOrganizationAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Delete, $"/api/v1/iam/organizations/{id}", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task<UserDto> CreateUserAsync(
      UserCreateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Post, "/api/v1/iam/users", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<UserDto>(cancellationToken);
   }

   public async Task<IReadOnlyCollection<UserLiteDto>> GetUsersByOrganizationAsync(
      Guid organizationId,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Get, $"/api/v1/iam/users/by-organization/{organizationId}", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<List<UserLiteDto>>(cancellationToken);
   }

   public async Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Get, "/api/v1/iam/users/me", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<UserDto>(cancellationToken);
   }

   public async Task UpdateMeAsync(
      UserUpdateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Put, "/api/v1/iam/users/me", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task UpdatePasswordAsync(
      UserUpdatePasswordRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Patch, "/api/v1/iam/users/me/password", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task DeleteUserAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Delete, $"/api/v1/iam/users/{id}", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   private async Task<HttpResponseMessage> SendAsync(
      HttpMethod method,
      string requestUri,
      object? body = null,
      CancellationToken cancellationToken = default)
   {
      using var request = new HttpRequestMessage(method, requestUri);
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

      if (body != null)
      {
         request.Content = JsonContent.Create(body);
      }

      return await _httpClient.SendAsync(request, cancellationToken);
   }
}
