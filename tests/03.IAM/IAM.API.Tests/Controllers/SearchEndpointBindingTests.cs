using IAM.API.Controllers;
using IAM.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.DTOs.Requests;

namespace IAM.API.Tests.Controllers;

public class SearchEndpointBindingTests
{
   [Theory]
   [InlineData(typeof(OrganizationController), nameof(OrganizationController.Get), typeof(OrganizationSearchRequest))]
   [InlineData(typeof(RoleController), nameof(RoleController.Get), typeof(RoleSearchRequest))]
   [InlineData(typeof(ParameterController), nameof(ParameterController.GetByParams), typeof(ParameterSearchRequest))]
   [InlineData(typeof(PermissionController), nameof(PermissionController.GetByParams), typeof(PermissionSearchRequest))]
   public void SearchEndpoints_ShouldBindRequestFromQuery(Type controllerType, string methodName, Type requestType)
   {
      var method = controllerType.GetMethod(methodName);
      var requestParameter = method!
         .GetParameters()
         .Single(parameter => parameter.ParameterType == requestType);

      var attribute = requestParameter.GetCustomAttributes(typeof(FromQueryAttribute), inherit: false);

      Assert.Single(attribute);
   }
}
