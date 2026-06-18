using Core.API.EndToEnd.Tests.Infrastructure;
using IAM.Domain;
using Shared.Domain.DTOs.Requests;

namespace Core.API.EndToEnd.Tests.Scenarios;

public class ParameterEndToEndTests(CoreApiTestFixture fixture) : IClassFixture<CoreApiTestFixture>
{
   private const string OverrideValue = "30";
   private const string DefaultValue = "90";

   private readonly CoreApiTestFixture _fixture = fixture;

   [Fact]
   public async Task ParameterFlow_ShouldOverrideReadDeleteOverrideAndReadDefaultValue()
   {
      var organizationRequest = ScenarioDataFactory.CreateOrganization();
      await _fixture.Api.CreateOrganizationAsync(
         organizationRequest,
         TestContext.Current.CancellationToken);
      var authenticatedApi = await _fixture.Api.LoginAsync(
         organizationRequest.User.Email,
         organizationRequest.User.Password,
         TestContext.Current.CancellationToken);

      var parameters = await authenticatedApi.SearchParametersAsync(
         new ParameterSearchRequest(null, null, null, IamParam.Security.MaxPasswordAgeInDays, null),
         TestContext.Current.CancellationToken);
      var parameter = parameters.Single(parameter => parameter.Key == IamParam.Security.MaxPasswordAgeInDays);

      await authenticatedApi.SaveParameterOverrideAsync(
         parameter.Id,
         new ParameterOwnerUpdateRequest(OverrideValue),
         TestContext.Current.CancellationToken);
      var overriddenValue = await authenticatedApi.GetParameterValueAsync(
         IamParam.Security.MaxPasswordAgeInDays,
         TestContext.Current.CancellationToken);

      Assert.NotNull(overriddenValue.ParameterOverrideId);

      await authenticatedApi.DeleteParameterOverrideAsync(overriddenValue.ParameterOverrideId.Value, TestContext.Current.CancellationToken);
      var defaultValue = await authenticatedApi.GetParameterValueAsync(
         IamParam.Security.MaxPasswordAgeInDays,
         TestContext.Current.CancellationToken);

      Assert.Equal(OverrideValue, overriddenValue.Value);
      Assert.True(overriddenValue.IsOverride);
      Assert.Equal(DefaultValue, defaultValue.Value);
      Assert.False(defaultValue.IsOverride);
      Assert.Null(defaultValue.ParameterOverrideId);
   }
}
