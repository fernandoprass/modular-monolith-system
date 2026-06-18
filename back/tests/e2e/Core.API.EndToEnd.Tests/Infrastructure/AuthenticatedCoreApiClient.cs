using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
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

   public async Task UpdateOrganizationAdminAsync(
      Guid id,
      UserUpdateOrganizationAdminRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Patch, $"/api/v1/iam/users/{id}/organization-admin", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task<RoleDto> CreateRoleAsync(
      RoleCreateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Post, "/api/v1/iam/roles", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<RoleDto>(cancellationToken);
   }

   public async Task UpdateRoleAsync(
      Guid id,
      RoleUpdateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Put, $"/api/v1/iam/roles/{id}", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task<IReadOnlyCollection<RoleDto>> SearchRolesAsync(
      RoleSearchRequest request,
      CancellationToken cancellationToken = default)
   {
      var query = new List<string>();

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         query.Add($"Name={Uri.EscapeDataString(request.Name)}");
      }

      if (request.UserId.HasValue)
      {
         query.Add($"UserId={request.UserId}");
      }

      if (request.IsActive.HasValue)
      {
         query.Add($"IsActive={request.IsActive.Value}");
      }

      var uri = "/api/v1/iam/roles";
      if (query.Count > 0)
      {
         uri = $"{uri}?{string.Join("&", query)}";
      }

      var response = await SendAsync(HttpMethod.Get, uri, cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<List<RoleDto>>(cancellationToken);
   }

   public async Task DeleteRoleAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Delete, $"/api/v1/iam/roles/{id}", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task AssignRoleAsync(
      RoleAssignRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Post, "/api/v1/iam/user-access/roles/assign", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task<IReadOnlyCollection<PermissionDto>> GetUserRolePermissionsAsync(
      Guid userId,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Get, $"/api/v1/iam/user-access/users/{userId}/permissions", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<List<PermissionDto>>(cancellationToken);
   }

   public async Task<IReadOnlyCollection<PermissionDto>> SearchPermissionsAsync(
      PermissionSearchRequest request,
      CancellationToken cancellationToken = default)
   {
      var query = new List<string>();

      if (request.roleId.HasValue)
      {
         query.Add($"roleId={request.roleId}");
      }

      if (!string.IsNullOrWhiteSpace(request.Module))
      {
         query.Add($"Module={Uri.EscapeDataString(request.Module)}");
      }

      if (!string.IsNullOrWhiteSpace(request.Resource))
      {
         query.Add($"Resource={Uri.EscapeDataString(request.Resource)}");
      }

      if (!string.IsNullOrWhiteSpace(request.Action))
      {
         query.Add($"Action={Uri.EscapeDataString(request.Action)}");
      }

      query.Add($"IncludeInactive={request.IncludeInactive}");

      var uri = "/api/v1/iam/permissions";
      if (query.Count > 0)
      {
         uri = $"{uri}?{string.Join("&", query)}";
      }

      var response = await SendAsync(HttpMethod.Get, uri, cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<List<PermissionDto>>(cancellationToken);
   }

   public async Task AssignPermissionsAsync(
      RolePermissionAssignRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Post, "/api/v1/iam/roles/permissions/assign", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task UnassignPermissionsAsync(
      RolePermissionUnassignRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Delete, "/api/v1/iam/roles/permissions/unassign", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task<PermissionCheckResponse> CheckPermissionAsync(
      PermissionCheckRequest request,
      CancellationToken cancellationToken = default)
   {
      using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/iam/authorization/check");
      httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
      httpRequest.Headers.Add("X-Internal-Api-Key", "test-key");
      httpRequest.Content = JsonContent.Create(request);

      var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.Content.ReadFromJsonAsync<PermissionCheckResponse>(cancellationToken)
         ?? throw new InvalidOperationException("Permission check response was empty.");
   }

   public async Task<IReadOnlyCollection<ParameterDto>> SearchParametersAsync(
      ParameterSearchRequest request,
      CancellationToken cancellationToken = default)
   {
      var query = new List<string>();

      if (!string.IsNullOrWhiteSpace(request.Module))
      {
         query.Add($"Module={Uri.EscapeDataString(request.Module)}");
      }

      if (!string.IsNullOrWhiteSpace(request.Group))
      {
         query.Add($"Group={Uri.EscapeDataString(request.Group)}");
      }

      if (!string.IsNullOrWhiteSpace(request.Name))
      {
         query.Add($"Name={Uri.EscapeDataString(request.Name)}");
      }

      if (!string.IsNullOrWhiteSpace(request.Key))
      {
         query.Add($"Key={Uri.EscapeDataString(request.Key)}");
      }

      if (!string.IsNullOrWhiteSpace(request.Title))
      {
         query.Add($"Title={Uri.EscapeDataString(request.Title)}");
      }

      if (!string.IsNullOrWhiteSpace(request.Description))
      {
         query.Add($"Description={Uri.EscapeDataString(request.Description)}");
      }

      var uri = "/api/v1/iam/parameters";
      if (query.Count > 0)
      {
         uri = $"{uri}?{string.Join("&", query)}";
      }

      var response = await SendAsync(HttpMethod.Get, uri, cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<List<ParameterDto>>(cancellationToken);
   }

   public async Task<ParameterValueDto> GetParameterValueAsync(
      string key,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Get, $"/api/v1/iam/parameters/value?key={Uri.EscapeDataString(key)}", cancellationToken: cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);

      return await response.ReadResultDataAsync<ParameterValueDto>(cancellationToken);
   }

   public async Task SaveParameterOverrideAsync(
      Guid id,
      ParameterOwnerUpdateRequest request,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Put, $"/api/v1/iam/parameters/{id}/override", request, cancellationToken);
      await response.EnsureSuccessStatusCodeAsync(cancellationToken);
   }

   public async Task DeleteParameterOverrideAsync(
      Guid id,
      CancellationToken cancellationToken = default)
   {
      var response = await SendAsync(HttpMethod.Delete, $"/api/v1/iam/parameters/{id}/override", cancellationToken: cancellationToken);
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
