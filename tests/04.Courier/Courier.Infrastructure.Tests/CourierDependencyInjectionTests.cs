using Courier.Infrastructure;
using Courier.Infrastructure.BackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Courier.Infrastructure.Tests;

public class CourierDependencyInjectionTests
{
   [Fact]
   public void AddCourierInfrastructure_WhenHostedServicesAreEnabled_ShouldRegisterWorkers()
   {
      var services = new ServiceCollection();
      var configuration = CreateConfiguration(hostedServicesEnabled: true);

      services.AddCourierInfrastructure(configuration);

      Assert.Contains(services, IsHostedService<CourierIndexInitializer>);
      Assert.Contains(services, IsHostedService<EmailRequestConsumer>);
      Assert.Contains(services, IsHostedService<EmailDeliveryWorker>);
   }

   [Fact]
   public void AddCourierInfrastructure_WhenHostedServicesAreDisabled_ShouldNotRegisterWorkers()
   {
      var services = new ServiceCollection();
      var configuration = CreateConfiguration(hostedServicesEnabled: false);

      services.AddCourierInfrastructure(configuration);

      Assert.DoesNotContain(services, IsHostedService<CourierIndexInitializer>);
      Assert.DoesNotContain(services, IsHostedService<EmailRequestConsumer>);
      Assert.DoesNotContain(services, IsHostedService<EmailDeliveryWorker>);
   }

   private static IConfiguration CreateConfiguration(bool hostedServicesEnabled)
   {
      return new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["ConnectionStrings:CourierDb"] = "mongodb://localhost:27017",
            ["ConnectionStrings:Redis"] = "localhost:6379",
            ["Courier:HostedServicesEnabled"] = hostedServicesEnabled.ToString()
         })
         .Build();
   }

   private static bool IsHostedService<TImplementation>(ServiceDescriptor descriptor)
   {
      return descriptor.ServiceType == typeof(IHostedService)
         && descriptor.ImplementationType == typeof(TImplementation);
   }
}
