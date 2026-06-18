using Courier.API.Controllers;
using IAM.API.Controllers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.API.Controllers;

namespace Core.API.Tests;

public class ControllerApplicationPartTests
{
   [Fact]
   public void CoreHostApplicationParts_ShouldExposeModuleControllers()
   {
      var services = new ServiceCollection();

      var mvcBuilder = services
         .AddControllers()
         .AddApplicationPart(typeof(UserController).Assembly)
         .AddApplicationPart(typeof(LogController).Assembly)
         .AddApplicationPart(typeof(EmailController).Assembly);

      var feature = new ControllerFeature();
      mvcBuilder.PartManager.PopulateFeature(feature);

      var controllerTypes = feature.Controllers.Select(controller => controller.AsType()).ToList();

      Assert.Contains(typeof(UserController), controllerTypes);
      Assert.Contains(typeof(LogController), controllerTypes);
      Assert.Contains(typeof(EmailController), controllerTypes);
      Assert.Contains(typeof(TemplateController), controllerTypes);
      Assert.DoesNotContain(controllerTypes, controller => controller.Namespace == "Core.API.Controllers");
   }
}
